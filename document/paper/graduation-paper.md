#基于机器视觉的人体运动类型识别  
##摘要  
本文提出用基于深度学习的机器视觉模型，在只使用由 Kinect 采集回来的人体骨架的运动数据的条件下进行人体运动类型的识别。使用由 Kinect 采集回来骨架模型是因为它具有数据容易获取，数据维度少的特点，可以方便存储、传输。而文章中的这个识别模型在训练时除了 CAD-60 数据集提供的人体骨架数据，没有使用任何的先验知识来进行人体运动类型的识别。这样子可以减少在数据的预处理阶段中人工的干预，只由机器自己来进行特征的提取，这样子可以减少人工提取特征时出现的错误。这也能使最终的模型的泛化性更优秀，鲁棒性更好。从而让模型对人体运动类型有更好的理解与识别。模型上，本文测试了卷积神经网络、自编码器、降噪自编码器、限制玻耳兹曼机以及混合的多种结构，并实验了多种网络训练上的用来加强效果的算法，比如一些正规化的方法和其他新的激活函数，最后选择了卷积神经网络作为自动特征提取的模型，并在其后面配合上多层感知机来进行分类。  

关键字：人体运动类型识别，CAD-60 数据集，深度学习，卷积神经网络，自编码器，限制玻耳兹曼机，Kinect  

#HUMAN ACTION RECOGNITION BASE ON COMPUTER VISION  
##Abstract  
We propose in the paper a computer vision model base on deep learning, which can recognise the human action only base on the data source in the skeletons of the human from Kinect. Using the skeletons from Kinect is because it is easy to get, to store and to transfer, and it has the less order of the data. This model only use the human skeletons data from the CAD-60 dataset to recognise the human antion without using any prior knowledge. It can reduce the works from the human on the stage of preprocessing and hand the feature extraction to the computer, which can reduce the error from the human-engineer. It can also improve generalization performance and robustness of the model, And give the a understanding of the human action. In the paper, we do the experiment on with convolutional neural networks, autoencoder, denoise autoencoder, Restricted Boltzmann Machines and the some of their mixture, and it also do some experiments for the tricks which can improve the nerual network, such as some regularization methods or other activation functions. In the end, we choose the convolutional neural networks for the feature extraction. And use the multilayer perceptron as the follow classifier.  

keywords: Human action recognition, CAD-60 dataset, deep models, convolutional neural networks, autoencoder, Restricted Boltzmann Machines, Kinect  

###绪论  
####引言  
现代社会快速的生活节奏和巨大的工作压力，严重影响着个人的身体健康。科学的运动可以提高身体素质增强运动能力，进而降低患病的风险(尤其是一些慢性疾病），例如糖尿病、血脂异常、高血压等。而进行科学运动的前提是实现人体运动类型的准确识别。
在信息安全领域，通过智能监控的方式利用计算机自动对视频中人体的运动类型进行识别从而为监控或者案件侦破提供依据也具有着重要的意义和广泛的用途。
在人机交互领域可以通过手势、身体姿态等信息对除了辅助交互输入设备以及自然语言分析进行补充，提高计算机和人进行交互的能力，使之更有趣。
在大型的图像数据库或者互联网上的信息中还可以利用人体运动类型的识别，对部分信息进行标注和理解，进而提供搜索和学习的能力。
本课题的任务是通过深度学习的相关理论，尝试用基于深度学习的机器视觉模型，应用于人体运动类型的识别，从而让模型对人体运动类型有更好的理解与识别。  

####人体运动类型识别的相关研究现状  
sdfsdf
sadfas
sdaf
sdf
sdf  