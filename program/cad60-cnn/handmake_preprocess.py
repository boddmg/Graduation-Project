#!/usr/bin/env python
__author__ = 'boddmg'


import numpy as np
from Preprocessor.Base_utils import *
from Preprocessor.encoder import *
import os


MAX_EPOCHS_UNSUPERVISED = 1
MAX_EPOCHS_SUPERVISED = 2

TRAIN_SCALE = 0.8
def printl(x):
    print(x)

class HandmakeDatasetLoader(Preprocessor):
    def __init__(self, data_type = "train"):
        self.data_type = data_type
        pass

    def run(self, src_data = None, src_labels = None):
        file_path = []
        for root, dirs, files in os.walk("./handmake_dataset/person2"):
            for file in files:
                if file.split(".")[1] == "pkl":
                    file_path += [os.path.join(root,file)]
        for i in file_path:
            print file_path.index(i), os.path.split(i)

        src_data = map(lambda x: np.load(x), file_path)
        src_labels = []
        for i in range(len(src_data)):
            # split into two set: train/test
            length = src_data[i].shape[0]
            start = 0 if self.data_type == "train" else length * TRAIN_SCALE
            start = int(start)
            end = length * TRAIN_SCALE if self.data_type == "train" else length
            end = int(end)
            src_data[i] = src_data[i][start:end]

            # create labels
            src_labels += [i]*(end - start)

            # reshape
            src_data[i] = src_data[i].reshape(src_data[i].shape[0], 1, src_data[i].shape[1])
        # fold data
        src_data = reduce(lambda x, y: np.concatenate((x, y), axis=0), src_data)
        src_labels = np.asarray(src_labels, dtype=np.uint8)
        return src_data, src_labels


def main():
    for i in ["train", "test"]:
        src_data, src_labels = PreprocessorList([
            HandmakeDatasetLoader(i),
            Monitor(),
            DataDump("handmake_%s.hkl" % i)
        ]).run()

    print src_data.shape, src_labels.shape

if __name__ == '__main__':
    main()

