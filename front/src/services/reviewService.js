import api from '../api/axios';

export const createReview = async ({ tourId, orderId, rating, commenting }) => {
  return api.post('/reviews', {
    tourId,
    orderId,
    rating,
    commenting
  });
};

export const getTourReviews = async (tourId) => {
  const res = await api.get(`/reviews/tour/${tourId}`);
  return res.data;
};
