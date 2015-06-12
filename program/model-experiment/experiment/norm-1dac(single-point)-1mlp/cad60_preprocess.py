#!/usr/bin/env python
__author__ = 'boddmg'
import sys
import os

sys.path.append(os.path.abspath(os.path.dirname(__file__)+'/'+'..'+'/'+".."))



from cad60_skeleton import CAD60
from Preprocessor.Base_utils import *
from Preprocessor.dataset_utils import *
from Preprocessor.encoder import *



class CAD60Loader(Preprocessor):
    def __init__(self, data_type = "train"):
        self.data_type = data_type
        pass

    def run(self, src_data = None, src_labels = None):
        src_data, src_labels, temp = CAD60(batch_size=1, data_type=self.data_type).get_data()
        return src_data, src_labels


def main():
    norm_param = {}

    for i in ["train", "test"]:
        src_data, src_labels = PreprocessorList([
            DataLoad("../../cad60_%s.hkl" % i),
            Index(CAD_60_POSE_INDEX),
            Normalization(norm_param, i == "test"),
            # SplitIntoBatches(48, 5),
            # PreFlattener(),
            Monitor(),
            denoising_autoencoder_encoder((144, 72), 3, "dac1.pkl", "dac1.pkl" if i == "test" else None),
            Monitor(),
            DataDump("cad60_%s_feature.hkl" % i)]).run()

    print src_data.shape, src_labels.shape

if __name__ == '__main__':
    main()

