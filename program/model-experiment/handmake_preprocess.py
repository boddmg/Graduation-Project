#!/usr/bin/env python
__author__ = 'boddmg'


from cad60_skeleton import CAD60
from Preprocessor.Base_utils import *
from Preprocessor.encoder import *


MAX_EPOCHS_UNSUPERVISED = 1
MAX_EPOCHS_SUPERVISED = 2

class HandmakeDatasetLoader(Preprocessor):
    def __init__(self, data_type = "train"):
        self.data_type = data_type
        pass

    def run(self, src_data = None, src_labels = None):
        src_data, src_labels, temp = CAD60(batch_size=1, data_type=self.data_type).get_data()
        print(src_data.shape)
        print(src_labels.shape)
        return src_data, src_labels


def main():
    for i in ["train", "test"]:
        src_data, src_labels = PreprocessorList([
            HandmakeDatasetLoader(i),
            Monitor()
        ]).run()

    print src_data.shape, src_labels.shape

if __name__ == '__main__':
    main()

