# module import
import math
import time
import numpy as np
import torch
import torch.nn as nn
import torchvision.transforms as transforms
from tqdm import tqdm
from Data import ImageDataset
from Model import ResNet
from Plot.MetricsPlot import *
from sklearn.metrics import classification_report

# global variable for updating the model
counter = 0
test_loss_min = np.Inf
correct_max = -1.0 * np.Inf

# global list to record average loss for training and testing of CNN
loss_list_train = []
loss_list_test = []
y_pred_all_list = []
y_test_all_list = []
classification_report_list = []

# training epoch
num_epochs = 5
# plot interval
plot_interval = 1

# training time
training_time = time.localtime()

# class distribution
class_label = ["class 0: \n(1.68, 61.666]", "class 1: \n(61.666, 109.787]", "class 2: \n(109.787, 156.359]",
               "class 3: \n(156.359, 191.443]", "class 4: \n(191.443, 334.059]"]


def train(model, loss_fn, optimizer, epoch, train_loader, device):
    """
    Description: training process of the CNN model

    :param model: the CNN model
    :param loss_fn: loss function
    :param optimizer: optimizer function
    :param epoch: current epoch of the training
    :param train_loader: train loader iterator
    :param log_interval: interval to print the log information
    :param device: whether the model is trained on GPU or CPU

    """
    # record the sum of the running loss
    running_loss = 0.0

    # State that you are training the model
    model.train(True)
    print("Training...")
    # use tqdm
    loop = tqdm(enumerate(train_loader, 0), total=len(train_loader), position=0)
    for batch_idx, data in loop:
        # record the number of correct item
        num_correct = 0
        # get the inputs; data is a list of [inputs, labels]
        inputs, labels = data[0].to(device), data[1].to(device)
        # zero the parameter gradients
        optimizer.zero_grad()
        # Forward propagation
        outputs = model(inputs)
        # calculate loss
        loss = loss_fn(outputs, labels)
        running_loss += loss.item()
        # back propagation
        loss.backward()
        _, pred = torch.max(outputs, 1)
        num_correct += (pred == labels).sum().item()
        running_train_acc = float(num_correct) / float(len(inputs))
        # update the parameter
        optimizer.step()
        loop.set_description(f'Epoch [{epoch}/{num_epochs}]')
        loop.set_postfix(loss=loss.item(), acc=running_train_acc)

    # add the average loss to the list
    loss_list_train.append(running_loss / len(train_loader.dataset) * 1.0)


def test(model, loss_fn, epoch, test_loader, device):
    """
    Description: testing process of the CNN model

    :param model: CNN model
    :param loss_fn: loss function
    :param epoch: current epoch for testing
    :param test_loader: test loader iterator
    :param device: whether to use GPU or CPU for calculation
    """
    global counter, test_loss_min, correct_max, training_time

    # State that you are testing the model; this prevents layers e.g. Dropout to take effect
    model.eval()
    # Init loss & correct & total & classnum prediction accumulators
    # correct for computing accuracy
    test_loss = 0
    correct = 0
    y_truth_list = []
    y_pred_list = []
    print("Testing...")

    with torch.no_grad():
        # Iterate over data
        for data in tqdm(test_loader, total=len(test_loader), position=0):
            images, labels = data[0].to(device), data[1].to(device)
            outputs = model(images)
            # Calculate & accumulate loss
            test_loss += loss_fn(outputs.data, labels).item()
            # get the index of the max log-probability (the predicted output label)
            _, pred = torch.max(outputs, 1)
            y_truth_list.append(data[1].cpu().numpy())
            y_pred_list.append(pred.cpu().numpy())
            correct += (pred == labels).sum().item()

    # Print log
    test_loss /= len(test_loader.dataset)
    loss_list_test.append(test_loss * 1.0)
    print('\nResult for the test set, Epoch {} , Average loss: {:.4f}, Accuracy: {}/{} ({:.0f}%)\n'.format(epoch,
                                                                                                           test_loss,
                                                                                                           correct,
                                                                                                           len(test_loader.dataset),
                                                                                                           100. * correct / len(
                                                                                                               test_loader.dataset)))
    if test_loss <= test_loss_min:
        print('Validation loss decreased ({:.6f} --> {:.6f}).  Saving model ...\n'.format(test_loss_min, test_loss))
        torch.save(model, 'checkpoint/resnet18_terrain_predictor_{}.pth'.format(time.strftime('%Y-%m-%d_%H-%M-%S',
                                                                                              training_time)))
        test_loss_min = test_loss
        counter = 0
        if correct_max == -1.0 * np.Inf:
            correct_max = correct
    elif correct > correct_max:
        print('Accuracy increased ({} --> {}).  Saving model ...\n'.format(100. * correct / len(test_loader.dataset),
                                                                           100. * correct_max / len(
                                                                               test_loader.dataset)))
        torch.save(model, 'checkpoint/resnet18_terrain_predictor_{}.pth'.format(time.strftime('%Y-%m-%d_%H-%M-%S',
                                                                                              training_time)))
        correct_max = correct
    else:
        counter += 1

    print("---------------------------------------------------------------------------------------\n")

    y_truth_new_list = np.concatenate(y_truth_list, axis=0)
    y_pred_new_list = np.concatenate(y_pred_list, axis=0)
    return y_truth_new_list, y_pred_new_list


if __name__ == '__main__':
    # stipulate the training device
    device = 'cuda' if torch.cuda.is_available() else 'cpu'
    # path for training set and testing set
    images_folder_train = 'train'
    images_folder_test = 'test'
    power_csv_file = 'data1_after_processing.csv'
    # batch size for training and testing
    train_batch_size = 16
    test_batch_size = 16
    # number of classes
    n_classes = 5
    # learning rate
    lr = 0.1

    # transform on raw data:
    # 1) change it to tensor
    # 2) normalization
    data_transforms = {
        'train': transforms.Compose([
            transforms.Resize(128),
            transforms.ToTensor(),
            transforms.Normalize([0.485, 0.456, 0.406], [0.229, 0.224, 0.225])
        ]),
        'test': transforms.Compose([
            transforms.Resize(128),
            transforms.ToTensor(),
            transforms.Normalize([0.485, 0.456, 0.406], [0.229, 0.224, 0.225])
        ]),
    }

    # load training and testing data
    trainset = ImageDataset.TerrainImageDataset(image_src=images_folder_train, powerdata_csv_path=power_csv_file,
                                                transform=data_transforms['train'])
    testset = ImageDataset.TerrainImageDataset(image_src=images_folder_test, powerdata_csv_path=power_csv_file,
                                               transform=data_transforms['test'])

    model = ResNet.resnet18(n_classes).to(device=device)
    loss_fn = nn.CrossEntropyLoss().to(device)

    # the train loader for the whole dataset
    train_loader = torch.utils.data.DataLoader(trainset, batch_size=train_batch_size, shuffle=True, num_workers=0)
    # the test loader for the whole dataset
    test_loader = torch.utils.data.DataLoader(testset, batch_size=test_batch_size, shuffle=False, num_workers=0)

    for epoch in range(1, num_epochs + 1):
        # 动态调整学习率
        if counter / 5 == 1:
            counter = 0
            lr = lr * 0.8
        # define the optimizer for updating the learning rate
        optimizer = torch.optim.SGD(model.parameters(), lr=lr, momentum=0.9, weight_decay=5e-4)
        train(model=model, loss_fn=loss_fn, optimizer=optimizer, epoch=epoch, train_loader=train_loader, device=device)
        y_test_list, y_pred_list = test(model=model, loss_fn=loss_fn, epoch=epoch, test_loader=test_loader,
                                        device=device)
        classification_report_list.append(classification_report(y_test_list,
                                                                y_pred_list,
                                                                labels=[0, 1, 2, 3, 4],
                                                                output_dict=True,
                                                                zero_division=0))
        if epoch % plot_interval == 0:
            y_test_all_list.append(y_test_list)
            y_pred_all_list.append(y_pred_list)

    # draw loss for train/test
    draw_loss(loss_list_train, loss_list_test, num_epochs=num_epochs, interval=plot_interval)
    # draw confusion matrix
    draw_confusion_matrix(y_test_all_list, y_pred_all_list, num_epochs=num_epochs, interval=plot_interval)
    # plot accuracy, f1_score, precision, for each class
    plotPerformanceForEachClass(classification_report_list,
                                n_classes=n_classes,
                                num_epochs=num_epochs,
                                interval=plot_interval,
                                metrics='all')
