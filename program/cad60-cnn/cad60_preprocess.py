#!/usr/bin/env python
__author__ = 'boddmg'


from cad60_skeleton import CAD60
from Preprocessor.Base_utils import *
from utilities.encoder import *


MAX_EPOCHS_UNSUPERVISED = 1
MAX_EPOCHS_SUPERVISED = 2

class CAD60Loader(Preprocessor):
    def __init__(self, data_type = "train"):
        self.data_type = data_type
        pass

    def run(self, src_data = None, src_labels = None):
        src_data, src_labels, temp = CAD60(batch_size=1, data_type=self.data_type).get_data()
        return src_data, src_labels


class Monitor(Preprocessor):
    def __init__(self):
        pass

    def run(self,src_data, src_labels):
        print(src_data.shape, src_labels.shape)
        return src_data, src_labels


def main():
    src_data, src_labels = PreprocessorList([
        DataLoad("cad60_train.hkl"),
        Monitor(),
        Encoder(get_layer_trainer_sgd_rbm,get_grbm([170,30]), 2),
        Monitor(),
        DataDump("cad60_train_feature.hkl")]).run()
    src_data, src_labels = PreprocessorList([
        DataLoad("cad60_train_feature.hkl"),
        Monitor(),
        Encoder(get_layer_trainer_sgd_rbm,get_grbm([30,10]), 3),
        DataDump("cad60_train_feature_feature.hkl")]).run()

    print src_data.shape, src_labels.shape

if __name__ == '__main__':
    main()
