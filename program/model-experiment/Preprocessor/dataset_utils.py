from pylearn2.datasets import dense_design_matrix

CAD_60_CONFIDENCE_INDEX = [9, 13, 23, 27, 37, 41, 51, 55, 65, 69, 79, 83, 93, 97, 107, 111, 121, 125, 135, 139,
                  149, 153, 157, 161, 165, 169]
CAD_60_POSE_INDEX = list(set(range(170)) - set(CAD_60_CONFIDENCE_INDEX))
CAD_60_POSE_INDEX.sort()

def src_2_datasetformat(src_data):
    return src_data.reshape(src_data.shape[0],
                         src_data.shape[1],
                         src_data.shape[2],1)

def datasetformat_2_src(datasetformat):
    return datasetformat.reshape(datasetformat.shape[0], 1, datasetformat.shape[1])

class Dataset(dense_design_matrix.DenseDesignMatrix):
    src_data = None
    src_labels = None

    def __init__(self, x, y,  **kwargs):
        y_labels = set()

        for i in y:
            y_labels.add(i[0])

        y_labels = len(y_labels)
        print(x.shape, y.shape, y_labels)

        super(Dataset, self).__init__(topo_view=x,
                                            y=y,
                                            y_labels=y_labels)

from fuel import config
from fuel.datasets import IndexableDataset
from fuel.utils import do_not_pickle_attributes
from collections import OrderedDict

@do_not_pickle_attributes('indexables')
class PackerForFuel(IndexableDataset):
    provides_sources = ('features', 'targets')

    def __init__(self, data, label, **kwargs):
        self.newData = OrderedDict(zip(self.provides_sources , [data, label[:, None]] ))
        super(PackerForFuel, self).__init__(self.newData, **kwargs)

    def load(self):
        self.indexables = [data for source, data
                           in self.newData
                           if source in self.sources]

