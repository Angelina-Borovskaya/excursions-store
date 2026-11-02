// components/Reviews/ReviewsSection.jsx
import React, { useEffect, useState } from 'react';
import ReviewItem from './ReviewItem';
import { getTourReviews } from '../../services/reviewService';
import './Reviews.css';

const ReviewsSection = ({ tourId }) => {
  const [reviews, setReviews] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadReviews();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [tourId]);

  const loadReviews = async () => {
    try {
      const data = await getTourReviews(tourId);
      setReviews(data.reviews);
    } catch (e) {
      console.error('Ошибка загрузки отзывов', e);
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="reviews">
      <h2 className="reviews-title">Отзывы</h2>

      {loading ? (
        <p className="reviews-loading">Загрузка отзывов...</p>
      ) : reviews.length === 0 ? (
        <p className="reviews-empty">Отзывов пока нет</p>
      ) : (
        <div className="reviews-list">
          {reviews.map(review => (
            <ReviewItem key={review.id} review={review} />
          ))}
        </div>
      )}
    </section>
  );
};

export default ReviewsSection;
