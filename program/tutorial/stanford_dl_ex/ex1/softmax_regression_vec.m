function [f,g] = softmax_regression_vec(theta, X,y)
    %
    % Arguments:
    %   theta - A vector containing the parameter values to optimize.
    %       In minFunc, theta is reshaped to a long vector.  So we need to
    %       resize it to an n-by-(num_classes-1) matrix.
    %       Recall that we assume theta(:,num_classes) = 0.
    %
    %   X - The examples stored in a matrix.  
    %       X(i,j) is the i'th coordinate of the j'th example.
    %   y - The label for each example.  y(j) is the j'th example's label.
    %
    m=size(X,2);
    n=size(X,1);

    % theta is a vector;  need to reshape to n x num_classes.
    theta=reshape(theta, n, []);
    num_classes=size(theta,2)+1;

    % initialize objective value and gradient.
    f = 0;
    g = zeros(size(theta));

    %
    % TODO:  Compute the softmax objective function and gradient using vectorized code.
    %        Store the objective function value in 'f', and the gradient in 'g'.
    %        Before returning g, make sure you form it back into a vector with g=g(:);
    %
    %%% YOUR CODE HERE %%%
    h = exp(theta' * X);
    p = h ./ repmat(sum(h,1),size(h,1),1);
    k = repmat( [1:size(h,1)]' , 1, size(h,2));
    k = bsxfun(@eq, k, repmat(y, size(h,1), 1));
    f = k .* log(p);
    f = -sum(sum(f));
    g = - X * (k - p)';
    %g = g';
    g=g(:); % make gradient a vector for minFunc
    