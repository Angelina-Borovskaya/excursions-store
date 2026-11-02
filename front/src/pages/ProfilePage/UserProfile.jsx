import React, { useEffect, useState } from 'react';
import './Profile.css';
import ReviewModal from '../../components/Reviews/ReviewModal';
import { useAuth } from '../../auth/AuthContext';
import { getMyOrders } from '../../services/orderService';

const UserProfile = () => {
  const { user } = useAuth();

  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [activeOrder, setActiveOrder] = useState(null);

  useEffect(() => {
    loadOrders();
  }, []);

  const loadOrders = async () => {
    try {
      const data = await getMyOrders();
      setOrders(data);
    } catch (e) {
      console.error('Ошибка загрузки заказов', e);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return <div className="profile-page">Загрузка...</div>;
  }

  return (
    <div className="profile-page">
      <div className="profile-container">
        <h1 className="profile-title">Профиль</h1>

        {/* USER INFO */}
        <div className="profile-card">
          <div className="profile-row">
            <span className="profile-label">Имя</span>
            <span>{user?.firstName}</span>
          </div>

          <div className="profile-row">
            <span className="profile-label">Email</span>
            <span>{user?.email}</span>
          </div>
        </div>

        {/* ORDERS */}
        <h2 className="profile-subtitle">История заказов</h2>

        {orders.length === 0 ? (
          <div className="profile-empty">
            У вас пока нет заказов
          </div>
        ) : (
          <div className="orders-list">
            {orders.map(order => (
              <div key={order.id} className="order-item">
                <div className="order-info">
                  <div className="order-title">
                    {order.tour?.title}
                  </div>

                  <div className="order-meta">
                    {new Date(order.orderDate).toLocaleDateString()} · {order.status}
                  </div>
                </div>

                {order.status === 'Завершён' && (
                  order.hasReview ? (
                    <span className="order-review-done">
                      Отзыв оставлен
                    </span>
                  ) : (
                    <button
                      className="order-review-btn"
                      onClick={() => setActiveOrder(order)}
                    >
                      Оставить отзыв
                    </button>
                  )
                )}

              </div>
            ))}
          </div>
        )}
      </div>

      {/* REVIEW MODAL */}
      {activeOrder && (
        <ReviewModal
          order={activeOrder}
          onClose={() => setActiveOrder(null)}
          onSuccess={loadOrders}
        />
      )}
    </div>
  );
};

export default UserProfile;
