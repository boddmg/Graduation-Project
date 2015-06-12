#!/usr/bin/env python
from fuel import config
from pylearn2.datasets import dense_design_matrix
import numpy
from cad60_skeleton import CAD60
from os import path

from utils import print_mem


class CAD60Skeleton(dense_design_matrix.DenseDesignMatrix):
    src_data = None
    src_labels = None

    def __init__(self, set_type = "train", batch_size = 10, shuffle = True, **kwargs):
        self.shuffle = shuffle
        self.set_type = set_type
        self.batch_size = batch_size

        x ,y ,y_labels= self._load_skeleton()
        self.y_labels = len(y_labels)

        super(CAD60Skeleton, self).__init__(topo_view=x,
                                            y=y,
                                            y_labels=self.y_labels)
        print_mem()

    def _load_skeleton(self):

        src_data, src_labels, labels_table= \
            CAD60(path.join(config.data_path, "cad60"),
                  self.batch_size,
                  self.set_type).get_data()

        if self.shuffle:
            indices = numpy.random.permutation(src_data.shape[0])
            dst_data = src_data[indices]
            del(src_data)
            dst_labels = src_labels[indices]
            del(src_labels)

        print "shape:", dst_data.shape, dst_labels.shape

        return dst_data.reshape(dst_data.shape[0],
                                dst_data.shape[1],
                                dst_data.shape[2],
                                1), dst_labels[:,None], labels_table


def main():
    print("start.")
    cad60_train = CAD60Skeleton("train", batch_size=50)
    cad60_test = CAD60Skeleton("test", batch_size=50)
    # next_batch = cad60.get_data_batch(10)
    # for i in next_batch:
    #     print(len(i[0][0]),i[1])

if __name__ == "__main__":
    main()
