import "./TaskCard.css";

function TaskCard({ task, onComplete, onEdit, onDelete }) {
  const formattedDate = new Intl.DateTimeFormat("pl-PL", {
    year: "numeric",
    month: "long",
    day: "2-digit",
  }).format(new Date(task.dueDate));

  const priorityLabels = {
    0: "Low",
    1: "Medium",
    2: "High",
  };

  return (
    <div className={`task-card ${task.completed ? "completed" : ""}`}>
      <div className="task-header">
        <h3>{task.title}</h3>

        <span
          className={`priority-badge priority-${priorityLabels[
            task.priority
          ].toLowerCase()}`}
        >
          {priorityLabels[task.priority]}
        </span>
      </div>

      <p className="task-date">
        <strong>Data wykonania:</strong> {formattedDate}
      </p>

      <p className="task-description">{task.description}</p>

      <div className="task-actions">
        {!task.completed && (
          <button className="complete-btn" onClick={() => onComplete(task.id)}>
            Oznacz jako wykonane
          </button>
        )}

        {!task.completed && (
          <button className="edit-btn" onClick={() => onEdit(task.id)}>
            Edytuj
          </button>
        )}

        <button className="delete-btn" onClick={() => onDelete(task.id)}>
          Usuń
        </button>
      </div>
    </div>
  );
}

export default TaskCard;
