from Data.Preprocessing import Preprocessing

if __name__ == '__main__':
    power_data_csv_path = 'data1.csv'
    src_folder_path = 'all'
    train_dst_folder_path = 'train'
    test_dst_folder_path = 'test'
    train_size = 5.0/6.0
    num_classes = 5
    after_processing_data_file = 'data1_after_processing.csv'

    p = Preprocessing(power_data_csv_path=power_data_csv_path,
                  src_folder_path=src_folder_path,
                  train_dst_folder_path=train_dst_folder_path,
                  test_dst_folder_path=test_dst_folder_path,
                  train_size=train_size,
                  num_classes=num_classes,
                  after_processing_data_file=after_processing_data_file)
    p.start()
