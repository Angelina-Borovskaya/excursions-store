import api from '../api/axios';

export const downloadSalesReport = async (startDate, endDate) => {
  const response = await api.post(
    '/reports/sales',
    { startDate, endDate },
    { responseType: 'blob' } 
  );

  return response.data;
};
