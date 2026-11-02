import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import ReviewsSection from '../../components/Reviews/ReviewsSection';
import { getTourById } from '../../services/tourService';
import { useFavorites } from '../../context/FavoritesContext';
import { useCart } from '../../context/CartContext';
import './ExcursionPage.css';
import { useAuthAction } from '../../auth/useAuthAction';


const API_ORIGIN = 'http://localhost:5066';

const ExcursionPage = () => {
  const authAction = useAuthAction();
  const { id } = useParams();

  const [excursion, setExcursion] = useState(null);
  const [loading, setLoading] = useState(true);

  // 🔥 КОНТЕКСТЫ
  const { isFavorite, toggleFavorite } = useFavorites();
  const { cartIds, toggleCart } = useCart();

  useEffect(() => {
    loadTour();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  const loadTour = async () => {
    try {
      const data = await getTourById(id);
      setExcursion(data);
    } catch (error) {
      console.error('Ошибка загрузки тура', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return <div className="excursion-page">Загрузка...</div>;
  }

  if (!excursion) {
    return <div className="excursion-page">Экскурсия не найдена</div>;
  }

  return (
    <div className="excursion-page">
      <h1 className="excursion-page__title">
        {excursion.title}
      </h1>

      <div className="excursion-page__main">
        {/* IMAGE */}
        <div className="excursion-page__image">
          <img
            src={
              excursion.imageFileName
                ? `${API_ORIGIN}/images/tours/${excursion.imageFileName}`
                : '/assets/images/placeholder.jpg'
            }
            alt={excursion.title}
            onError={(e) => {
              e.currentTarget.onerror = null;
              e.currentTarget.src = '/assets/images/placeholder.jpg';
            }}
          />

          <button
            className="excursion-page__favorite"
            onClick={() =>
              authAction(() => toggleFavorite(excursion.id))
            }
          >
            <img
              src={
                isFavorite(excursion.id)
                  ? '/assets/icons/heart2.svg'
                  : '/assets/icons/like.svg'
              }
              alt="Избранное"
            />
          </button>
        </div>

        {/* INFO */}
        <div className="excursion-page__info-block">
          <div className="excursion-page__location">
            {excursion.city?.country?.name}, {excursion.city?.name}
          </div>

          <div className="excursion-page__info">
            <span className="excursion-page__price">
              {excursion.price.toLocaleString()} ₽
            </span>
            <span className="excursion-page__rating">
              ★ {excursion.averageRating?.toFixed(1) || '—'}
            </span>
          </div>

          <div className="excursion-page__dates">
            <div>Дата начала: {excursion.startDate.slice(0, 10)}</div>
            <div>Дата окончания: {excursion.endDate.slice(0, 10)}</div>
          </div>

          <button
            className={`excursion-page__add-btn ${
              cartIds.has(excursion.id)
                ? 'excursion-page__add-btn--added'
                : ''
            }`}
            onClick={() =>
              authAction(() => toggleCart(excursion.id))
            }
          >
            {cartIds.has(excursion.id)
              ? 'Убрать из корзины'
              : 'В корзину'}
          </button>
        </div>
      </div>

      <div className="excursion-page__description">
        {excursion.description}
      </div>

      <section className="excursion-page__reviews">
        <ReviewsSection tourId={excursion.id} />
      </section>
    </div>
  );
};

export default ExcursionPage;
