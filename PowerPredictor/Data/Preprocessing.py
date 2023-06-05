import random
import shutil
import time

import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
import os
from tqdm import tqdm
import multiprocessing


# The Preprocessing class performs various image preprocessing tasks such as filtering, labeling, and
# relocating images based on their power distribution. 
# After using the preprocessing pre-defined function, all images in the src_folder will be labeled
# according to the distribution of the power in the whole dataset, and then relocate to train_folder and
# test folder
class Preprocessing:
    """
        :param power_data_csv_path: The file path of the CSV file containing the power data
        :type power_data_csv_path: str
        
        :param src_folder_path: The path to the folder containing the source images
        :type src_folder_path: str
        
        :param train_dst_folder_path: The path to the folder where the training images will be saved
        :type train_dst_folder_path: str
        
        :param test_dst_folder_path: The path to the destination folder where the test data will be
        saved
        :type test_dst_folder_path: str
        
        :param train_size: The proportion or number of samples to be used for training the model. It can
        be either an integer representing the number of samples or a float representing the proportion
        of samples to be used for training
        :type train_size: int or float
        
        :param num_classes: The number of classes or categories in the dataset. For example, if the
        dataset contains images of cats and dogs, the number of classes would be 2
        :type num_classes: int
        
        :param after_processing_data_file: The name of the file where the processed data will be saved.
        It is an optional parameter with a default value of 'data_after_preprocessing.csv', defaults to
        data_after_preprocessing.csv
        :type after_processing_data_file: str (optional)
    """
    def __init__(self,
                 power_data_csv_path: str,
                 src_folder_path: str,
                 train_dst_folder_path: str,
                 test_dst_folder_path: str,
                 train_size: int or float,
                 num_classes: int,
                 after_processing_data_file: str = 'data_after_preprocessing.csv'):

        self.class_name_list = None
        self.current_bins = None
        self.actual_bins = None
        self.filtered_data = None
        self.original_data = None
        self.labelled_data = None
        self.visualized_data = None
        self.data_total = None
        self.image_extension = ['.png', '.PNG', '.jpg', '.JPG']
        self.power_data_csv_path = power_data_csv_path
        self.src_folder_path = src_folder_path
        self.train_dst_folder_path = train_dst_folder_path
        self.test_dst_folder_path = test_dst_folder_path
        self.train_size = train_size
        self.num_classes = num_classes
        self.processed_data_file = after_processing_data_file
        self.filepath_dict = dict()

    def _get_info(self, filename):
        """
        This function takes a filename as input, extracts information from it, and returns a tuple of
        three elements.
        
        :param filename: The input parameter is a string representing a filename
        :return: a tuple containing three elements: the first element is the first part of the filename
        (indicating the terrainID), the second element is the second part of the filename (indicating position parameter), 
        and the third element is the third part of the filename (indicating direction parameter).
        """
        elements = filename.split('_')
        if elements[1] == '0' or elements[1] == '1' or elements[1] == '2' or elements[1] == '3':
            if elements[2] == '18':
                elements[2] = '4'
            elif elements[2] == '19':
                elements[2] = '5'
            elif elements[2] == '20':
                elements[2] = '6'

        return elements[0], elements[1], elements[2]

    def _get_data(self, search_df, terrainID, position, direction):
        """
        This function takes a search dataframe, terrainID, position, and direction as inputs and returns
        a filtered dataframe based on those inputs.
        
        :param search_df: This is a pandas DataFrame that contains data to be searched
        :param terrainID: The ID of the terrain being searched for in the dataframe
        :param position: Position of the car
        :param direction: Direction of the car
        :return: A pandas DataFrame filtered by the input parameters `terrainID`, `position`, and `direction` 
        from the `search_df` DataFrame.
        """
        df = search_df.loc[(search_df['terrainID'] == terrainID) &
                           (search_df['position'] == int(position)) &
                           (search_df['direction'] == int(direction))]
        return df

    def _get_power_group(self, df, terrainID, position, direction):
        """
        This function takes a dataframe, terrainID, position, and direction as inputs and returns the
        power group value from the dataframe that matches the input criteria.
        
        :param df: a pandas DataFrame containing information about power groups in different terrains
        :param terrainID: The ID of the terrain being queried for power group
        :param position: The position parameter is an integer value representing the position of the car
        in the terrain
        :param direction: The direction parameter is an integer value that represents the direction of
        the car.
        
        :return: an integer value which is the 'power_group' value from the input dataframe 'df' based
        on the input parameters 'terrainID', 'position', and 'direction'.
        """
        return int(df.loc[(df['terrainID'] == terrainID) & (df['position'] == int(position)) & (
                    df['direction'] == int(direction)), 'power_group'])

    def _is_in_dataframe(self, search_df, terrainID, position, direction):
        """
        This function checks if a certain data exists in a given dataframe based on terrain ID,
        position, and direction.
        
        :param search_df: This parameter is a pandas DataFrame that is being searched for a specific
        value
        :param terrainID: It is a variable that represents the ID of the terrain being searched for in
        the dataframe
        :param position: The position parameter is an integer value representing the position of the car
        in the terrain
        :param direction: The direction parameter is an integer value that represents the direction of
        the car.
        
        :return: a boolean value indicating whether the given parameters exist in the `search_df` dataframe
        """
        return not self._get_data(search_df, terrainID, position, direction).empty

    def _count_file(self, path):
        """
        This function counts the number of files with specific image extensions [['.png', '.PNG', '.jpg', '.JPG']] 
        in a given directory and its subdirectories recursively.
        
        :param path: The path parameter is a string that represents the directory path where the
        function will count the number of image files
        
        :return: the count of valid files with image extensions [['.png', '.PNG', '.jpg', '.JPG']] 
        in the given directory path and its subdirectories.
        """
        count = 0
        for item in os.listdir(path):
            if os.path.isfile(os.path.join(path, item)):
                if any(item.endswith(extension) for extension in self.image_extension):
                    count += 1
            else:
                count += self._count_file(os.path.join(path, item))
        return count

    def _get_sort_class_list(self, qcut_df_without_label):
        """
        This function takes a dataframe and returns a sorted list of class names based on the unique
        values in a specific column.
        
        :param qcut_df_without_label: It is a pandas DataFrame that contains the data after applying the
        pandas qcut function. The qcut function is used to divide the data into equal-sized bins based 
        on the specified number of quantiles. The qcut_df_without_label DataFrame does not contain any labels (labels=None),
        but the Interval (see documentation: https://pandas.pydata.org/docs/reference/api/pandas.qcut.html)
        
        :return: a sorted list of class names as strings.
        """
        interval_list = pd.unique(qcut_df_without_label.loc[:, 'power_group'])
        class_name_list_temp = []
        for i in range(len(interval_list)):
            class_name_list_temp.append(
                pd.Interval(np.round(interval_list[i].left, 3), np.round(interval_list[i].right, 3)))
        return np.sort(class_name_list_temp).astype(str)

    def _generate_random_color_list(self, length):
        """
        This function generates a a specified length long list of random RGB color tuples (R, G, B).
        
        :param length: An integer that specifies the number of random colors to
        generate in the color_list
        
        :return: A list of tuples, where each tuple contains three random values between 0 and 1
        representing the RGB values of a color.
        """
        color_list = []
        for i in range(length):
            color_list.append((np.random.rand(), np.random.rand(), np.random.rand()))
        return color_list

    def _establish_image_link_helper(self, df, src, dst, kind='train'):
        """
        This function helps establish image indexing by locating and matching files in a source directory
        with information in a dataframe, and then linking to a path of destination directories.
        
        :param df: a pandas dataframe containing information about the images (terrainID, position,
        direction, and power group)
        :param src: The source directory where the image files are located
        :param dst: dst stands for destination and refers to the directory where the image files will be
        copied to
        :param kind: The kind parameter specifies whether the function is looking for training or
        testing image files, by defaults train.
        """
        print("[INFO] locating {}ing files".format(kind))
        with tqdm(total=len(df), position=0) as pbar:
            for root, dirs, files in os.walk(src):
                for filename in files:
                    if any(filename.endswith(extension) for extension in self.image_extension):
                        terrainID, position, direction = self._get_info(filename)
                        if self._is_in_dataframe(df, terrainID, position, direction):
                            self.filepath_dict[os.path.join(root, filename)] = \
                                os.path.join(dst, str(self._get_power_group(df, terrainID, position, direction)))
                            pbar.update(1)

    def _copy_file(self, src, dst):
        """
        This function copies a file from a source path to a destination path using the shutil module in
        Python.
        
        :param src: The source file path that needs to be copied
        :param dst: dst stands for "destination" and refers to the path or directory where the file will
        be copied to
        """
        shutil.copy(src, dst)

    def _relocate_image_multi_process(self, num_processes):
        """
        This function copies files from source to destination folders 
        it utilizes multiprocessing in Python.
        """
        print("[INFO] Copying files ...")
        pbar = tqdm(total=self.data_total, position=0)
        pbar.set_description('File copy')
        update = lambda *args: pbar.update()

        # check dst folder exists or not
        for i in range(self.num_classes):
            if not os.path.exists(os.path.join(self.train_dst_folder_path, str(i))):
                print("[WARN] Path in train destination folder not exist, create: {}".format(
                    os.path.join(self.train_dst_folder_path, str(i))))
                os.mkdir(os.path.join(self.train_dst_folder_path, str(i)))
            if not os.path.exists(os.path.join(self.test_dst_folder_path, str(i))):
                print("[WARN] Path in test destination folder not exist, create: {}".format(
                    os.path.join(self.test_dst_folder_path, str(i))))
                os.mkdir(os.path.join(self.test_dst_folder_path, str(i)))
        # create multiprocessing pool
        pool = multiprocessing.Pool(num_processes)
        for src, dst in self.filepath_dict.items():
            pool.apply_async(self._copy_file, args=(src, dst, ), callback=update)

        pool.close()
        pool.join()
        print("[INFO] Copying files finished")

    def load(self):
        """
        This function loads the original power data from a CSV file
        """
        print('[INFO] Read original power data')
        self.original_data = pd.read_csv(self.power_data_csv_path)

    def filter(self):
        """
        This function filters the original_data by checking the src_folder
        It checks whether all the images have a corresponding energy in the CSV
        It also sends out warning if the image's corresponding power data cannot
        be found in the provided CSV file
        
        :returns a filtered dataset
        """
        print("[INFO] Filter the image manually selected...")
        self.filtered_data = self.original_data.copy()
        self.filtered_data.drop(self.filtered_data.index, inplace=True)
        self.data_total = self._count_file(self.src_folder_path)
        if self.data_total == 0:
            print("[Error] No file in the current file folder")
            raise Exception("Error: Zero images have been found in the provided file folder")
        with tqdm(total=self.data_total) as pbar:
            for root, dirs, files in os.walk(self.src_folder_path):
                for filename in files:
                    if any(filename.endswith(extension) for extension in self.image_extension):
                        terrainID, position, direction = self._get_info(filename)
                        df = self._get_data(self.original_data, terrainID, position, direction)
                        if df.empty:
                            print("[WARNING] No corresponding power data has been found in the given CSV file: {}".format(filename))
                        self.filtered_data = pd.concat([df, self.filtered_data], ignore_index=True)
                        pbar.update(1)

        print("[INFO] Filter successful, whole dataset size: {}".format(self.data_total))

    def hist(self):
        """
        This function draws a histogram of the power distribution of a filtered dataset and displays it.
        """

        print("[INFO] Drawing histogram of the power distribution")
        fig, ax = plt.subplots()
        N, self.current_bins, patches = ax.hist(self.filtered_data.loc[:, 'average_power'], bins=100, alpha=0.7)
        plt.title('Average electric power (in mW) \ndistribution histogram of the collected dataset')
        plt.xlabel('Power(mW)')
        plt.ylabel('Frequency')
        plt.show()

    def labeling(self):
        """
        This function assigns labels to a dataset based on the distribution of the average power and
        saves the processed data to a file.
        """
        print("[INFO] Assigning labels to the dataset ...")
        self.labelled_data, self.visualized_data = self.filtered_data.copy(), self.filtered_data.copy()
        self.labelled_data['power_group'], bins = pd.qcut(self.filtered_data['average_power'], self.num_classes,
                                                          labels=False, retbins=True)
        self.visualized_data['power_group'], self.actual_bins = pd.qcut(self.filtered_data['average_power'],
                                                                        self.num_classes, retbins=True)
        print("[INFO] The distribution of the average power is shown as belows: ")
        print(self.visualized_data.loc[:, 'power_group'].value_counts())
        self.class_name_list = self._get_sort_class_list(self.visualized_data)
        self.labelled_data = self.labelled_data.sort_values(by="power_group", ascending=True)
        self.labelled_data.to_csv(self.processed_data_file)
        print("[INFO] Dataset after processing has been saved to {}".format(self.processed_data_file))

    def hist_after_labeling(self):
        """
        This function draws a histogram of the filtered data with color-coded patches
        after the labelling is finished
        """
        print("[INFO] Drawing histogram after classification")
        start = 0
        end = start
        index_list = []
        for i in range(1, len(self.actual_bins)):
            for j in range(end, len(self.current_bins)):
                if self.actual_bins[i] <= self.current_bins[j]:
                    index_list.append((start, j))
                    start = j
                    end = start
                    break
        # start drawing
        fig, ax = plt.subplots()
        N, bins, patches = ax.hist(self.filtered_data.loc[:, 'average_power'], bins=100, alpha=0.7)
        color_list = self._generate_random_color_list(self.num_classes)
        patch_list = []
        for i in range(len(index_list)):
            for j in range(index_list[i][0], index_list[i][1]):
                patches[j].set_fc(color_list[i])
            patch_list.append(mpatches.Patch(color=color_list[i],
                                             label='group {}: {}'.format(i, self.class_name_list[i])))

        plt.legend(handles=patch_list)
        plt.title('Average electric power (in mW) distribution histogram \n'
                  'of the collected dataset with classified groups')
        plt.xlabel('Power(mW)')
        plt.ylabel('Frequency')
        plt.show()

    def relocate_image(self, num_processes=5):
        """
        This function relocates images from a source folder to train and test destination folders using
        multiple processes.
        
        :param num_processes: The number of worker processes to use for relocating images. It is an optional
        parameter with a default value of 5. If you don't know the concrete number of processes is acceptable
        you can use multiprocessing.cpu_count() as a reference
        """
        if 0 <= self.train_size <= 1:
            self.train_size = self.train_size * self.data_total
        train_df_list = []
        test_df_list = []
        for i in range(self.num_classes):
            df = self.labelled_data.loc[self.labelled_data['power_group'] == i]
            train_df = df.sample(n=int(np.floor(self.train_size / self.num_classes)),
                                 random_state=1, replace=False, axis=0)
            test_df = df[~df.index.isin(train_df.index)]
            train_df_list.append(train_df)
            test_df_list.append(test_df)
        train_df = pd.concat(train_df_list)
        test_df = pd.concat(test_df_list)

        self._establish_image_link_helper(train_df, self.src_folder_path, self.train_dst_folder_path, kind='train')
        self._establish_image_link_helper(test_df, self.src_folder_path, self.test_dst_folder_path, kind='test')

        self._relocate_image_multi_process(num_processes)

    def start(self):
        self.load()
        self.filter()
        self.hist()
        self.labeling()
        self.hist_after_labeling()
        self.relocate_image(num_processes=5)
