<>
#基于机器视觉的人体运动类型识别  
##摘要  
本文提出用基于深度学习的机器视觉模型，在只使用由 Kinect 采集回来的人体骨架的运动数据的条件下进行人体运动类型的识别。使用由 Kinect 采集回来骨架模型是因为它具有数据容易获取，数据维度少的特点，可以方便存储、传输。而文章中的这个识别模型在训练时除了 CAD-60 数据集提供的人体骨架数据，没有使用任何的先验知识来进行人体运动类型的识别。这样子可以减少在数据的预处理阶段中人工的干预，只由机器自己来进行特征的提取，这样子可以减少人工提取特征时出现的错误。这也能使最终的模型的泛化性更优秀，鲁棒性更好。从而让模型对人体运动类型有更好的理解与识别。模型上，本文测试了卷积神经网络、自编码器、降噪自编码器、限制玻耳兹曼机以及混合的多种结构，并实验了多种网络训练上的用来加强效果的算法，比如一些正规化的方法和其他新的激活函数，最后选择了卷积神经网络作为自动特征提取的模型，并在其后面配合上多层感知机来进行分类。  

关键字：人体运动类型识别，CAD-60 数据集，深度学习，卷积神经网络，自编码器，限制玻耳兹曼机，Kinect  

#HUMAN ACTION RECOGNITION BASE ON COMPUTER VISION  
##Abstract  
We propose in the paper a computer vision model base on deep learning, which can recognise the human action only base on the data source in the skeletons of the human from Kinect. Using the skeletons from Kinect is because it is easy to get, to store and to transfer, and it has the less order of the data. This model only use the human skeletons data from the CAD-60 dataset to recognise the human antion without using any prior knowledge. It can reduce the works from the human on the stage of preprocessing and hand the feature extraction to the computer, which can reduce the error from the human-engineer. It can also improve generalization performance and robustness of the model, And give the a understanding of the human action. In the paper, we do the experiment on with convolutional neural networks, autoencoder, denoise autoencoder, Restricted Boltzmann Machines and the some of their mixture, and it also do some experiments for the tricks which can improve the nerual network, such as some regularization methods or other activation functions. In the end, we choose the convolutional neural networks for the feature extraction. And use the multilayer perceptron as the follow classifier.  

keywords: Human action recognition, CAD-60 dataset, deep models, convolutional neural networks, autoencoder, Restricted Boltzmann Machines, Kinect  

##绪论  
###引言  
计算机自动去理解人类的行为、动作还有跟环境之间的交流互动，在近年来逐渐地成为了一个热门的领域，因为这个技术在很多地方都有可以使用的地方。比如现代社会快速的生活节奏和巨大的工作压力，严重影响着个人的身体健康。科学的运动可以提高身体素质增强运动能力，进而降低患病的风险(尤其是一些慢性疾病），例如糖尿病、血脂异常、高血压等。而进行科学运动的前提是实现人体运动类型的准确识别。
在信息安全领域，通过智能监控的方式利用计算机自动对视频中人体的运动类型进行识别从而为监控或者案件侦破提供依据也具有着重要的意义和广泛的用途。
在人机交互领域可以通过手势、身体姿态等信息对除了辅助交互输入设备以及自然语言分析进行补充，提高计算机和人进行交互的能力，使之更有趣。
在大型的图像数据库或者互联网上的信息中还可以利用人体运动类型的识别，对部分信息进行标注和理解，进而提供搜索和学习的能力。
本课题的任务是通过使用深度学习的相关理论，尝试构造用基于深度学习的机器视觉模型，应用于人体运动类型的识别，从而让模型对人体运动类型有更好的理解与识别。  

###人体运动类型识别的相关研究现状  
####人体运动数据源的获得  
首先，对于人体运动识别的数据的来源存在着几种不同的种类[1]。例如由 RGB 摄像机、距离传感器或其他遥感的方式。使用深度传感器来进行人体运动识别的发展始于 80 年代初。过去的研究主要集中在学习和认识到从视频序列(可见光相机）所采集的数据中。可见光视频的主要问题是从单目视频传感器捕捉得到的人体运动存在相当大的损失。由于视频天生的对的人体行为识别的限制，尽管已经有了过去几十年的努力，但通过视频来识别人体运动，仍然是非常具有挑战性的。
而得益于近期发布的成本低廉的深度传感器，我们看到了和 3D 数据相关的研究越来越多了。从过去的 20 年里，我们获得 3D 数据的方法，一共分为三类。一种方法是通过使用基于标记的动作捕捉系统，如 MoCap。 第二种方式是通过立体视觉： 从多个角度捕获 2D 图像序列，通过从多个视图来重建三维信息。第三种方式是使用距离传感器（使用类似 TOF 原理的传感器）。深度相机在过去几年里取得迅速的发展。最近出现的深度照相机可以在相对低廉的成本和较小的尺寸里给我们提供较高的帧率和分辨率，这导致出现了许多新的研究中的动作识别都是采用的三维数据。  
  
####人体动作识别的主要问题  
基于视觉的人体动作识别里有四个主要的问题。第一个问题的挑战性比较小 [2]  [3]：闭塞、 杂乱的背景、 阴影和不同光照条件会让运动难以分割或者被错误地识别。这是从 RGB 视频行为识别的一大难点。引入 3D 数据可以在很大程度上通过提供现场的结构信息，从而缓解这个问题。第二个是视角的变换[2] [4] [5] 和 [6]。相同的操作可以从不同的角度产生不同的"外观"。传统的 RGB 相机解决这一问题的方法主要是引入多个同步的摄像机，同时获得多个视角的图像，但对于某些应用程序，这不是件容易的事。不过对于三维运动捕捉系统，这不是一个严重的问题。而如果通过深度图像来进行识别的话，这个问题也会有部分被缓解，因为从轻微旋转的视角的外观可以推断深度的信息。这一点并不完全解决问题，因为摄像机始终还只是在对象的一侧上，这个距离图像只提供了部分的信息，还是没有人知道这个对象的另一面是什么样子的。如果可以运用单一深度相机来精确地推断出人的骨架模型，则可以通过骨架模型的信息来构造一种视图不变识别的算法。第三个问题是放缩上的差异，因为人离相机的距离的不同会影响主体的大小从而影响运动的识别。而在 RGB 视频中，这可以通过在多个尺度下的[7] 特征提取解决了。而在深度视频中，这可以很容易调整，因为真正的主体的 3D 尺寸直接是已知的。第四个问题是同一种类内的变异性和不同种类之间的相似性问题[8]。人可以通过不同的身体部位在不同的方向上做动作，但不同的方向和两个动作仅只有只由非常细微的细节来区分。而这个不管对于使用哪种数据的来源的算法来说都仍然是一个非常困难的问题。  
前三个问题基本上都可以通过使用三维空间上的图像或者类似骨架之类的模型来解决，所以本文使用的数据来源是由传感器采集计算直接得到的骨架数据。  
####人体动作识别的研究现状  


  
###引用文献  
[1] Chen L, Wei H, Ferryman J. A survey of human motion analysis using depth imagery. Pattern Recog Lett. 2013;34(15):1995-2006.

[2] D. Weinland, M. Özuysal, P. Fua, Making action recognition robust to occlusions and viewpoint changes, ECCV. Springer (2010), pp. 635–648

[3] L.C. Chen, J.W. Hsieh, C.H. Chuang, C.Y. Huang, D.Y. Chen, Occluded human action analysis using dynamic manifold model, ICPR, IEEE (2012), pp. 1245–1248

[4] M.B. Holte, T.B. Moeslund, N., Nikolaidis, I. Pitas, 3d human action recognition for multi-view camera systems, in: 3DIMPVT, 2011, pp. 342–349

[5] D. Weinland, E. Boyer, R. Ronfard, Action recognition from arbitrary views using 3d exemplars, ICCV, IEEE (2007), pp. 1–7

[6] M.C. Roh, H.K. Shin, S.W. Lee, View-independent human action recognition with volume motion template on single stereo camera, Pattern Recognit. Lett., 31 (2010), pp. 639–647

[7] M.Y. Chen, A. Hauptmann, Mosift: Recognizing human actions in surveillance videos, 2009.

[8] R. Poppe, A survey on vision-based human action recognition, Image Vision Comput., 28 (2010), pp. 976–990

[9] R. Bellman, Dynamic Programming. Princeton, NJ: Princeton Univ. Press, 1957. 

[10] R. Duda, P. Hart, and D. Stork, Pattern Recognition, 2nd ed. New York: Wiley-Interscience, 2000. 

[11] Y. LeCun, L. Bottou, Y. Bengio, and P. Haffner, “Gradient-based learning applied to document recognition,” Proc. IEEE, vol. 86, no. 11, pp. 2278–2324, 1998.

[12] F.-J. Huang and Y. LeCun, “Large-scale learning with SVM and convolutional nets for generic object categorization,” in Proc. Computer Vision and Pattern Recognition Conf. (CVPR’06), 2006.  
