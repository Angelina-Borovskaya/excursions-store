import React, { useState } from 'react';
import './ReviewModal.css';
import { createReview } from '../../services/reviewService';

const ReviewModal = ({ order, onClose, onSuccess }) => {
  const [rating, setRating] = useState(0);
  const [text, setText] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      setLoading(true);

      await createReview({
        tourId: order.tourId,
        orderId: order.id,
        rating,
        commenting: text
      });

      onClose();
      onSuccess?.();
    } catch (e) {
      console.error('Ошибка отправки отзыва', e);
      alert('Не удалось отправить отзыв');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="review-modal__overlay">
      <div className="review-modal">
        <button className="review-modal__close" onClick={onClose}>×</button>

        <h2 className="review-modal__title">Оставить отзыв</h2>

        <p className="review-modal__subtitle">
          Экскурсия: <strong>{order.tour?.title}</strong>
        </p>

        <form onSubmit={handleSubmit} className="review-form">
          <div className="review-form__rating">
            <span>Оценка:</span>
            <div className="stars">
              {[1,2,3,4,5].map(star => (
                <button
                  key={star}
                  type="button"
                  className={`star ${rating >= star ? 'active' : ''}`}
                  onClick={() => setRating(star)}
                >
                  ★
                </button>
              ))}
            </div>
          </div>

          <textarea
            className="review-form__textarea"
            placeholder="Напишите ваш отзыв..."
            value={text}
            onChange={e => setText(e.target.value)}
            required
          />

          <button
            type="submit"
            className="review-form__submit"
            disabled={rating === 0 || loading}
          >
            {loading ? 'Отправка...' : 'Отправить отзыв'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default ReviewModal;
