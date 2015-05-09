#!/usr/bin/env python
__author__ = 'boddmg'
import sys
import os

sys.path.append(os.path.abspath(os.path.dirname(__file__)+'/'+'..'+'/'+".."))



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
        src_data, src_labels, temp = CAD60(batch_size=1,
                                           data_type=self.data_type).get_data()
        return src_data, src_labels


def main():
    for i in ["train", "test"]:
        src_data, src_labels = PreprocessorList([
            DataLoad("../../cad60_%s.hkl" % i),
            Encoder(get_layer_trainer_sgd_rbm,
                    get_grbm([170,20]), 20,
                    "grbm1.pkl",
                    "grbm1.pkl" if i == "test" else None),
            SplitIntoBatches(72,5),
            Flattener(),
            Monitor(),
            Encoder(get_layer_trainer_sgd_rbm,
                    get_grbm([20*72,30]), 20,
                    "grbm2.pkl",
                    "grbm2.pkl" if i == "test" else None),
            Monitor(),
            Shuffle(),
            Monitor(),
            DataDump("cad60_%s_feature.hkl" % i)]).run()

    print src_data.shape, src_labels.shape

if __name__ == '__main__':
    main()

