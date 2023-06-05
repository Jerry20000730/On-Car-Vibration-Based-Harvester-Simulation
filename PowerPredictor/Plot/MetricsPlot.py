import matplotlib.pyplot as plt
import time
import math
import numpy as np
from sklearn.metrics import ConfusionMatrixDisplay


def draw_loss(loss_list_train, loss_list_test, num_epochs, interval):
    """
    This function draws a plot of train and test loss against epochs for a predictor model.
    
    :param loss_list_train: A list of training loss values for each epoch during training
    :param loss_list_test: A list of test set loss values for each epoch during training
    :param num_epochs: The number of epochs (iterations) the model is trained for
    :param interval: The interval parameter is the number of epochs between each tick on the x-axis of
    the plot. It is used to control the spacing of the ticks and make the plot more readable

    :returns train_test_loss plot
    """

    print("\nDrawing train/test loss for predictor")
    plt.cla()
    x1 = [str(i + 1) for i in range(num_epochs)]
    y1 = loss_list_train
    y2 = loss_list_test
    plt.title('Train loss / test loss vs. epochs in predictor', fontsize=10)
    plt.plot(x1, y1, '.-', color='b', label="train set loss")
    plt.plot(x1, y2, '--', color='r', label="test set loss")
    plt.xlabel('epochs', fontsize=8)
    plt.ylabel('loss', fontsize=8)
    plt.xticks(range(0, num_epochs, interval))
    plt.grid()
    plt.legend()
    plt.savefig('Result/train_test_loss_{}_epochs_{}.png'.format(num_epochs,
                                                                 time.strftime('%Y-%m-%d_%H-%M-%S', time.localtime())),
                dpi=500, bbox_inches='tight')
    plt.show()


def draw_confusion_matrix(y_test_list, y_pred_list, num_epochs, interval):
    """
    This function draws a normalized confusion matrix for a given number of epochs and intervals.
    
    :param y_test_list: A list of (iterations of) true labels for the test set
    :param y_pred_list: A list of (iterations of) predicted labels for the test set
    :param num_epochs: The total number of epochs for which the confusion matrix will be plotted
    :param interval: The interval parameter specifies the number of epochs between each confusion matrix plot

    :returns plot normalized confusion matrix for total number of epochs
    """

    h = int(math.ceil(num_epochs / interval / 4.0))
    fig, axes = plt.subplots(nrows=h, ncols=4, figsize=(12, int(h * 3)))
    fig.tight_layout()
    fig.subplots_adjust(top=0.9, wspace=0.2, hspace=0.3)
    fig.suptitle('Normalized confusion matrix over {} epochs'.format(num_epochs), verticalalignment='top')
    for i in range(len(y_test_list)):
        if h == 1:
            axes[i % 4].set_title('epoch {}'.format(int(i * interval + 1)))
            ConfusionMatrixDisplay.from_predictions(y_test_list[i], y_pred_list[i], normalize='true',
                                                    ax=axes[i % 4])
        else:
            axes[int(i / 4), i % 4].set_title('epoch {}'.format(int(i * interval + 1)))
            ConfusionMatrixDisplay.from_predictions(y_test_list[i], y_pred_list[i], normalize='true',
                                                    ax=axes[int(i / 4), i % 4])

    for j in range(len(y_test_list), int(h * 4)):
        if h == 1:
            axes[j % 4].set_axis_off()
        else:
            axes[int(j / 4), j % 4].set_axis_off()
    plt.savefig('Result/confusion_matrix_{}.png'.format(time.strftime('%Y-%m-%d_%H-%M-%S',
                                                                      time.localtime())))
    plt.show()


def plotPerformanceForEachClass(classification_report_list, n_classes, num_epochs, interval, metrics):
    """
    This function plots the performance evaluation for a given set of classification reports, number of
    classes, epochs, interval, and metrics.
    
    :param classification_report_list: A list of classification reports for each epoch
    :param n_classes: The number of classes in the classification problem
    :param num_epochs: The number of epochs is the number of times the algorithm will iterate over the
    entire training dataset during the training process
    :param interval: The interval parameter specifies the interval between the epochs to be plotted on
    the x-axis of the performance evaluation graph. For example, if interval=5, then only every 5th
    epoch will be plotted on the x-axis
    :param metrics: The performance metric to plot. It can be 'accuracy', 'f1 score', 'precision',
    'recall', or 'all', meaning plotting all.

    :returns 
    """

    f1_score_list, precision_score_list, recall_score_list, accuracy_score_list = _seperate_classification_list(
        classification_report_list, n_classes, num_epochs)
    class_list = []
    epoch_list = [str(i + 1) for i in range(num_epochs)]

    if metrics == 'all':
        plotPerformanceForEachClass(classification_report_list, n_classes=n_classes, num_epochs=num_epochs,
                                    interval=interval, metrics='accuracy')
        plotPerformanceForEachClass(classification_report_list, n_classes=n_classes, num_epochs=num_epochs,
                                    interval=interval, metrics='f1 score')
        plotPerformanceForEachClass(classification_report_list, n_classes=n_classes, num_epochs=num_epochs,
                                    interval=interval, metrics='precision')
        plotPerformanceForEachClass(classification_report_list, n_classes=n_classes, num_epochs=num_epochs,
                                    interval=interval, metrics='recall')
    else:
        if metrics == 'accuracy':
            plt.figure(figsize=(10, 5))
            plt.plot(epoch_list, accuracy_score_list, color='red', marker='o')
            plt.title('testing accuracy for predictor over {} epochs'.format(num_epochs))
            plt.xlabel('epoch')
            plt.ylabel('accuracy')
            plt.grid()
            plt.savefig('Result/performance_evaluation_for_{}_{}.png'.format(metrics, time.strftime('%Y-%m-%d_%H-%M-%S',
                                                                                                    time.localtime())))
            plt.show()
        else:
            if metrics == 'f1 score':
                class_list = f1_score_list
            elif metrics == 'precision':
                class_list = precision_score_list
            elif metrics == 'recall':
                class_list = recall_score_list

            h = int(math.ceil(n_classes / 5.0))
            plt.figure(figsize=(14, int(h * 4)))
            for i in range(n_classes):
                plt.subplot(h, 5, i + 1)
                plt.plot(epoch_list, class_list[i], color='blue', marker='v')
                plt.title("{} for class {}".format(metrics, i))
                plt.xlabel('epoch')
                plt.ylabel(metrics)
                plt.xticks(range(0, num_epochs, interval))
            plt.suptitle('{} for the predictor over {} epochs'.format(metrics, num_epochs))
            plt.savefig('Result/performance_evaluation_for_{}_{}.png'.format(metrics, time.strftime('%Y-%m-%d_%H-%M-%S',
                                                                                                    time.localtime())))
            plt.show()


def _seperate_classification_list(classification_list, n_classes, num_epochs):
    """
    This function separates classification metrics from a list of dictionaries into separate lists for
    f1-score, precision, recall, and accuracy.
    
    :param classification_list: A list of dictionaries containing classification metrics for each epoch
    using the API provided by scikit-learn: classcification_report
    :param n_classes: The number of classes in the classification problem
    :param num_epochs: The number of epochs is the number of times the entire dataset is passed through
    the neural network during training. It is a hyperparameter that is set before training the model
    :return: four lists: f1_score_list, precision_score_list, recall_score_list, and
    accuracy_score_list. These lists contain the evaluation metrics (f1-score, precision, recall, and
    accuracy) for each class and epoch, extracted from a list of dictionaries called
    classification_list.
    """

    f1_score_list, precision_score_list, recall_score_list = [[] for _ in range(num_epochs)], \
        [[] for _ in range(num_epochs)], [[] for _ in range(num_epochs)]
    accuracy_score_list = []
    for dict in classification_list:
        accuracy_score_list.append(np.round(dict.get('accuracy'), 2))
        for i in range(n_classes):
            f1_score_list[i].append(np.round(dict['{}'.format(i)].get('f1-score'), 2))
            precision_score_list[i].append(np.round(dict['{}'.format(i)].get('precision'), 2))
            recall_score_list[i].append(np.round(dict['{}'.format(i)].get('recall'), 2))

    return f1_score_list, precision_score_list, recall_score_list, accuracy_score_list
