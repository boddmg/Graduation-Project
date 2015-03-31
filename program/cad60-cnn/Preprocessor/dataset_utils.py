from pylearn2.datasets import dense_design_matrix

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
