import React from 'react';
import './ExcursionCard.css';
import { Link } from 'react-router-dom';

const API_ORIGIN = 'http://localhost:5066';

const ExcursionCard = ({
  excursion,
  isFavorite,
  inCart,
  onToggleFavorite,
  onToggleCart
}) => {
  const imageSrc = excursion.imageFileName
    ? `${API_ORIGIN}/images/tours/${excursion.imageFileName}`
    : '/assets/images/placeholder.jpg';

  const location = `${excursion.city?.country?.name || '—'}, ${excursion.city?.name || '—'}`;

  const rating = excursion.averageRating != null
    ? excursion.averageRating.toFixed(1)
    : '—';

  return (
    <div className="excursion-card">
      <div className="excursion-card__image-wrapper">
        <Link to={`/tour/${excursion.id}`} className="excursion-card__link">
         <img
            src={imageSrc}
            alt={excursion.title}
            className="excursion-card__image"
            loading="lazy"
            onError={(e) => {
              e.currentTarget.onerror = null;
              e.currentTarget.src = '/assets/images/placeholder.jpg';
            }}
          />
        </Link>

        <div className="excursion-card__favorite">
          <button
            type="button"
            onClick={(e) => {
              e.preventDefault();
              e.stopPropagation();
              onToggleFavorite(excursion.id);
            }}
            title={isFavorite ? 'Удалить из избранного' : 'Добавить в избранное'}
          >
            <img
              src={isFavorite ? '/assets/icons/heart2.svg' : '/assets/icons/like.svg'}
              alt="Избранное"
            />
          </button>
        </div>
      </div>

      <Link to={`/tour/${excursion.id}`} className="excursion-card__link">
        <div className="excursion-card__body">
          <h3 className="excursion-card__title">{excursion.title}</h3>

          <p className="excursion-card__location">{location}</p>

          <div className="excursion-card__info">
            <span className="excursion-card__price">
              {Number(excursion.price || 0).toLocaleString()} ₽
            </span>

            <span className="excursion-card__rating">★ {rating}</span>
          </div>
        </div>
      </Link>

      <div className="excursion-card__body">
        <button
          type="button"
          className={`excursion-card__button ${inCart ? 'excursion-card__button--added' : ''}`}
          onClick={() => onToggleCart(excursion.id)}
        >
          {inCart ? 'Убрать из корзины' : 'В корзину'}
        </button>
      </div>
    </div>
  );
};

export default ExcursionCard;
