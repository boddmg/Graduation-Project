#!/usr/bin/env python
__author__ = 'boddmg'
import sys
import os

sys.path.append(os.path.abspath(os.path.dirname(__file__)+'/'+'..'+'/'+".."))

from cad60_skeleton import CAD60
from Preprocessor.Base_utils import *


class CAD60Loader(Preprocessor):
    def __init__(self, data_type = "train"):
        self.data_type = data_type
        pass

    def run(self, src_data = None, src_labels = None):
        src_data, src_labels, temp = CAD60(batch_size=1, data_type=self.data_type).get_data()
        return src_data, src_labels


def main():
    for i in ["train", "test"]:
        src_data, src_labels = PreprocessorList([
            DataLoad("../../cad60_%s.hkl" % i),
          #  Encoder(get_layer_trainer_sgd_autoencoder,
                    #get_denoising_autoencoder([170,100]), 5,
                    #"ac1.pkl",
          #          "ac1.pkl" if i == "test" else None),
            Monitor(),
            DataDump("cad60_%s_feature.hkl" % i)]).run()

    print src_data.shape, src_labels.shape

if __name__ == '__main__':
    main()

