import "./TaskModal.css";
import { useEffect, useState } from "react";

function TaskModal({ isOpen, onClose, onSave, task }) {
  const [formData, setFormData] = useState({
    title: "",
    date: "",
    description: "",
  });

  useEffect(() => {
    if (task) {
      setFormData({
        title: task.title,
        date: task.date,
        description: task.description,
      });
    } else {
      setFormData({
        title: "",
        date: "",
        description: "",
      });
    }
  }, [task]);

  if (!isOpen) return null;

  const handleChange = (e) => {
    setFormData((prev) => ({
      ...prev,
      [e.target.name]: e.target.value,
    }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    onSave({
      ...task,
      ...formData,
    });

    onClose();
  };

  return (
    <div className="modal-overlay">
      <div className="task-modal">
        <h2>{task ? "Edytuj zadanie" : "Dodaj zadanie"}</h2>

        <form onSubmit={handleSubmit}>
          <input
            type="text"
            name="title"
            placeholder="Tytuł"
            value={formData.title}
            onChange={handleChange}
            required
          />

          <input
            type="date"
            name="date"
            value={formData.date}
            onChange={handleChange}
            required
          />

          <textarea
            name="description"
            placeholder="Opis"
            value={formData.description}
            onChange={handleChange}
            rows="5"
          />

          <div className="modal-actions">
            <button type="submit" className="save-btn">
              Zapisz
            </button>

            <button type="button" className="cancel-btn" onClick={onClose}>
              Anuluj
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default TaskModal;
