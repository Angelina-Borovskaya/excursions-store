import { useState } from 'react';
import { downloadSalesReport } from '../../services/reportService';
import './ReportModal.css';

const ReportModal = ({ onClose }) => {
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [loading, setLoading] = useState(false);

  const handleDownload = async (e) => {
    e.preventDefault();

    if (!startDate || !endDate) {
      alert('Выберите период');
      return;
    }

    try {
      setLoading(true);

      const blob = await downloadSalesReport(startDate, endDate);

      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `sales-report_${startDate}_${endDate}.pdf`;
      a.click();

      window.URL.revokeObjectURL(url);
      onClose();
    } catch (e) {
      console.error(e);
      alert('Ошибка формирования отчёта');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="report-modal__overlay">
      <div className="report-modal">
        <button
          className="report-modal__close"
          onClick={onClose}
        >
          ×
        </button>

        <h2 className="report-modal__title">
          Отчёт по продажам
        </h2>

        <p className="report-modal__subtitle">
          Выберите период для формирования PDF-отчёта
        </p>

        <form className="report-form" onSubmit={handleDownload}>
          <div className="report-form__dates">
            <div className="report-form__field">
              <label>Дата начала</label>
              <input
                type="date"
                value={startDate}
                onChange={e => setStartDate(e.target.value)}
              />
            </div>

            <div className="report-form__field">
              <label>Дата окончания</label>
              <input
                type="date"
                value={endDate}
                onChange={e => setEndDate(e.target.value)}
              />
            </div>
          </div>

          <button
            type="submit"
            className="report-form__submit"
            disabled={loading || !startDate || !endDate}
          >
            {loading ? 'Формирование...' : 'Скачать PDF'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default ReportModal;
