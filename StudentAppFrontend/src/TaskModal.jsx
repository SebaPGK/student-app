import "./TaskModal.css";
import { useEffect, useState } from "react";

function TaskModal({ isOpen, onClose, onSave, task }) {
  const [formData, setFormData] = useState({
    title: "",
    dueDate: "",
    description: "",
    priority: 0,
  });

  useEffect(() => {
    if (task) {
      setFormData({
        title: task.title,
        dueDate: task.dueDate,
        description: task.description,
        priority: task.priority,
      });
    } else {
      setFormData({
        title: "",
        dueDate: "",
        description: "",
        priority: 0,
      });
    }
  }, [task]);

  if (!isOpen) return null;

  const handleChange = (e) => {
    const { name, value } = e.target;

    setFormData((prev) => ({
      ...prev,
      [name]: name === "priority" ? Number(value) : value,
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
            name="dueDate"
            value={formData.dueDate}
            onChange={handleChange}
            required
          />

          <select
            name="priority"
            value={formData.priority}
            onChange={handleChange}
          >
            <option value={0}>Niski</option>
            <option value={1}>Średni</option>
            <option value={2}>Wysoki</option>
          </select>

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
