#!/usr/bin/env python
__author__ = 'boddmg'


from cad60_skeleton import CAD60
from preprocess import *

class CAD60Loader(Preprocessor):
    def __init__(self):
        pass

    def run(self, src_data = None, src_labels = None):
        src_data, src_labels, temp = CAD60(batch_size=1, data_type="train").get_data()
        return src_data, src_labels

def main():
    src_data, src_labels = PreprocessorList([CAD60Loader(),DataDump("default2.hkl")]).run()
    print src_data.shape, src_labels.shape

if __name__ == '__main__':
    main()