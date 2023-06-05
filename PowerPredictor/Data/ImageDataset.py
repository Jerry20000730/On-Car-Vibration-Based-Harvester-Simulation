from torch.utils.data import Dataset
from PIL import Image
import pandas as pd
import os


# This is a PyTorch dataset class for loading terrain images and their corresponding power group data
# from a CSV file.
class TerrainImageDataset(Dataset):
    def __init__(self, image_src, powerdata_csv_path, transform=None):
        super().__init__()
        self.image_src = os.path.abspath(image_src)
        self.transforms = transform
        image_extension = ['.png', '.PNG', '.jpg', '.JPG']
        images = []
        pwdf = pd.read_csv(powerdata_csv_path)
        for dirname in os.listdir(image_src):
            for filename in os.listdir(os.path.join(image_src, dirname)):
                if any(filename.endswith(extension) for extension in image_extension):
                    terrainID, position, direction = self.get_info(filename)
                    power_group = self.query_power_group(pwdf, terrainID=terrainID, position=position,
                                                         direction=direction)
                    images.append((os.path.abspath(os.path.join(image_src, dirname, filename)), power_group))
        self.images = images

    def __len__(self):
        """
        This function returns the number of the images in the dataset
        
        :return: The length of the "images" attribute of the object.
        """
        return len(self.images)

    def __getitem__(self, index):
        """
        This function returns an image and its corresponding power group from a list of images, with an
        optional transformation (see transforms tutorial: https://pytorch.org/tutorials/beginner/basics/transforms_tutorial.html) 
        applied to the image.
        
        :param index: The index parameter is the index of the item that needs to be retrieved from the
        dataset. In this case, it is used to retrieve the filepath and power_group of the image at the
        specified index
        
        :return: A tuple containing the transformed image and the power group associated with the image
        at the given index.
        """
        filepath, power_group = self.images[index]
        img = Image.open(filepath).convert('RGB')
        if self.transforms:
            img = self.transforms(img)
        return img, power_group

    def get_info(self, filename):
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
    
    def query_power(self, df, terrainID, position, direction):
        """
        This function takes a dataframe, terrainID, position, and direction as inputs and returns the
        average power from the dataframe.
        
        :param df: a pandas DataFrame containing power data for different terrains, positions, and
        directions
        :param terrainID: The ID of the terrain being queried for power data
        :param position: The position parameter is an integer value representing the position of the car.
        :param direction: The direction parameter is an integer that represents the direction of the car.
        
        :return: a float value of the 'average_power' column from the input dataframe 'df' based on the
        conditions specified in the query. The conditions are that the 'terrainID' column should match
        the input 'terrainID', the 'position' column should match the input 'position' (converted to an
        integer), and the 'direction' column should match the input 'direction'.
        """
        return float(df.loc[(df['terrainID'] == terrainID) & (df['position'] == int(position)) & (df['direction'] == int(direction)), 'average_power'])

    def query_power_group(self, df, terrainID, position, direction):
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
