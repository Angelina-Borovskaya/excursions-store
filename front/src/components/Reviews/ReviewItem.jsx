// components/Reviews/ReviewItem.jsx
import React from 'react';

const ReviewItem = ({ review }) => {
  return (
    <div className="review-item">
      <div className="review-header">
        <span className="review-author">
          {review.clientName || 'Аноним'}
        </span>

        <span className="review-date">
          {new Date(review.createdAt).toLocaleDateString()}
        </span>
      </div>

      <div className="review-rating">
        {'★'.repeat(review.rating)}
        {'☆'.repeat(5 - review.rating)}
      </div>

      <p className="review-text">
        {review.commenting}
      </p>
    </div>
  );
};

export default ReviewItem;
