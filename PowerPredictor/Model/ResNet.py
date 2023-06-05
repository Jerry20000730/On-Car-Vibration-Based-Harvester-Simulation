import torch
import torch.nn as nn
import math


def conv3x3(in_planes, out_planes, stride=1):
    """
    This function returns a 3x3 convolutional layer with specified input and output channels, stride,
    padding, and bias.
    
    :param in_planes: The number of input channels to the convolutional layer
    :param out_planes: The number of output channels produced by the convolutional layer
    :param stride: Stride refers to the number of pixels the convolutional filter moves at each step
    while scanning the input image. In this case, the stride is set to 1, which means that the filter
    moves one pixel at a time.
    :return: A 2D convolutional layer with a kernel size of 3x3, input channels of `in_planes`, output
    channels of `out_planes`, a stride of `stride`, padding of 1, and no bias term.
    """
    return nn.Conv2d(in_planes, out_planes, kernel_size=3, stride=stride,
                     padding=1, bias=False)


# This is a basic block module for a ResNet architecture in PyTorch, which performs convolution, batch
# normalization, and residual connections.
class BasicBlock(nn.Module):

    def __init__(self, in_channels, out_channels, stride=1, expansion=1, downsample=None):
        super(BasicBlock, self).__init__()
        self.expansion = expansion
        self.downsample = downsample
        self.conv1 = conv3x3(in_channels, out_channels, stride)
        self.bn1 = nn.BatchNorm2d(out_channels)
        self.relu = nn.ReLU(inplace=True)
        self.conv2 = conv3x3(out_channels, out_channels * self.expansion)
        self.bn2 = nn.BatchNorm2d(out_channels * self.expansion)

    def forward(self, x):
        residual = x

        out = self.conv1(x)
        out = self.bn1(out)
        out = self.relu(out)

        out = self.conv2(out)
        out = self.bn2(out)

        if self.downsample is not None:
            residual = self.downsample(x)

        out += residual
        out = self.relu(out)

        return out


# This is a ResNet class implementation in PyTorch for image classification tasks.
class ResNet(nn.Module):

    def __init__(self, img_channels, num_layers, block, num_classes=1000):
        super(ResNet, self).__init__()
        if num_layers == 18:
            layers = [2, 2, 2, 2]
            self.expansion = 1
        elif num_layers == 34:
            layers = [3, 4, 6, 3]
            self.expansion = 1

        self.in_channels = 64
        # All ResNets (18 to 152) contain a Conv2d => BN => ReLU for the first
        # three layers. Here, kernel size is 7.
        self.conv1 = nn.Conv2d(img_channels, self.in_channels, kernel_size=7, stride=2, padding=3,
                               bias=False)
        self.bn1 = nn.BatchNorm2d(self.in_channels)
        self.relu = nn.ReLU(inplace=True)
        self.maxpool = nn.MaxPool2d(kernel_size=3, stride=2, padding=1)

        self.layer1 = self._make_layer(block, 64, layers[0])
        self.layer2 = self._make_layer(block, 128, layers[1], stride=2)
        self.layer3 = self._make_layer(block, 256, layers[2], stride=2)
        self.layer4 = self._make_layer(block, 512, layers[3], stride=2)

        self.avgpool = nn.AdaptiveAvgPool2d((1, 1))
        self.fc = nn.Linear(512 * self.expansion, num_classes)

    def _make_layer(self, block, out_channels, blocks, stride=1):
        downsample = None
        if stride != 1:
            downsample = nn.Sequential(
                nn.Conv2d(self.in_channels, out_channels * self.expansion,
                          kernel_size=1, stride=stride, bias=False),
                nn.BatchNorm2d(out_channels * self.expansion),
            )

        layers = []
        layers.append(block(self.in_channels, out_channels, stride, self.expansion, downsample))
        self.in_channels = out_channels * self.expansion
        for i in range(1, blocks):
            layers.append(block(self.in_channels, out_channels, expansion=self.expansion))

        return nn.Sequential(*layers)

    def forward(self, x):
        x = self.conv1(x)
        x = self.bn1(x)
        x = self.relu(x)
        x = self.maxpool(x)

        x = self.layer1(x)
        x = self.layer2(x)
        x = self.layer3(x)
        x = self.layer4(x)

        x = self.avgpool(x)
        x = torch.flatten(x, 1)
        x = self.fc(x)

        return x


def resnet18(num_classes):
    """
    This function returns a ResNet-18 model with a specified number of output classes.
    
    :param num_classes: The number of classes in the classification task. For example, if you are
    classifying images into 10 different categories, num_classes would be 10
    :return: A ResNet model with 18 layers and BasicBlock as the
    building block, which takes in 3-channel images and outputs predictions for `num_classes` classes.
    """
    model = ResNet(img_channels=3, num_layers=18, block=BasicBlock, num_classes=num_classes)
    return model

def resnet34(num_classes):
    """
    This function returns a ResNet-34 model with a specified number of output classes.
    
    :param num_classes: The number of classes in the classification task. For example, if you are
    classifying images into 10 different categories, num_classes would be 10
    :return: A ResNet model with 34 layers and a specified number of
    output classes.
    """
    model = ResNet(img_channels=3, num_layers=34, block=BasicBlock, num_classes=num_classes)
    return model
