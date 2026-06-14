import "./DashboardPage.css";
import Header from "./Header";
import TaskCard from "./TaskCard";
import TaskModal from "./TaskModal";
import { useState, useEffect } from "react";
import {
  deleteTaskById,
  getTasks,
  updateTask,
  createTask,
} from "./services/taskService";

function DashboardPage() {
  const [loading, setLoading] = useState(false);
  const [apiError, setApiError] = useState("");
  const [selectedFilter, setSelectedFilter] = useState("todo");
  const [tasks, setTasks] = useState([]);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingTask, setEditingTask] = useState(null);

  const user = JSON.parse(localStorage.getItem("user") ?? "{}");

  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    window.location.href = "/login";
  };

  useEffect(() => {
    if (loading) {
      document.body.style.overflow = "hidden";
    } else {
      document.body.style.overflow = "auto";
    }
  }, [loading]);

  useEffect(() => {
    const fetchTasks = async () => {
      setLoading(true);
      setApiError("");
      try {
        const data = await getTasks();

        setTasks(data);
      } catch (error) {
        setApiError("Cannot retrieve tasks from database");
        console.log(error);
      } finally {
        setLoading(false);
      }
    };
    fetchTasks();
  }, []);

  const toggleComplete = async (id) => {
    setLoading(true);
    setApiError("");
    try {
      const selectedTask = tasks.find((task) => task.id === id);

      const updatedTask = {
        ...selectedTask,
        completed: !selectedTask.completed,
      };

      await updateTask(updatedTask);

      setTasks((prevTasks) =>
        prevTasks.map((task) => (task.id === id ? updatedTask : task)),
      );
    } catch (error) {
      setApiError("Could not update task");
      console.error(error);
    }
    setLoading(false);
  };

  const toggleDelete = async (id) => {
    setLoading(true);
    setApiError("");
    try {
      await deleteTaskById(id);
      setTasks((prevTasks) => prevTasks.filter((task) => task.id !== id));
    } catch (error) {
      setApiError("Could not delete task");
      console.error(error);
    }
    setLoading(false);
  };

  const handleAddTask = () => {
    setEditingTask(null);
    setIsModalOpen(true);
  };

  const handleEditTask = (id) => {
    const task = tasks.find((task) => task.id === id);

    setEditingTask(task);
    setIsModalOpen(true);
  };

  const handleSaveTask = async (taskData) => {
    setLoading(true);
    setApiError("");
    if (taskData.id) {
      try {
        await updateTask(taskData);

        setTasks((prev) =>
          prev.map((task) => (task.id === taskData.id ? taskData : task)),
        );
      } catch (error) {
        setApiError("Could not update task");
        console.log(error);
      }
    } else {
      try {
        taskData.completed = false;
        const newTask = await createTask(taskData);
        setTasks((prev) => [...prev, newTask]);
      } catch (error) {
        setApiError("Could not add new task into database");
        console.log(error);
      }
    }
    setLoading(false);
  };

  const today = new Date();
  const todoTasks = tasks.filter((task) => !task.completed).length;
  const lateTasks = tasks.filter((task) => {
    return !task.completed && new Date(task.dueDate) < today;
  }).length;
  const completedTasks = tasks.filter((task) => task.completed).length;
  const allTasks = tasks.length;

  const filteredTasks = tasks.filter((task) => {
    switch (selectedFilter) {
      case "todo":
        return !task.completed;
      case "late":
        return !task.completed && new Date(task.dueDate) < today;
      case "completed":
        return task.completed;
      default:
        return true;
    }
  });

  return (
    <>
      <Header user={user} onLogout={handleLogout} onAddTask={handleAddTask} />
      <div className="dashboard">
        {apiError && <div className="api-error">{apiError}</div>}
        <div className="stats-container">
          <div
            className={`stat-card todo ${
              selectedFilter === "todo" ? "active" : ""
            }`}
            onClick={() => setSelectedFilter("todo")}
          >
            <h3>Do wykonania</h3>
            <p>{todoTasks}</p>
          </div>

          <div
            className={`stat-card late ${
              selectedFilter === "late" ? "active" : ""
            }`}
            onClick={() => setSelectedFilter("late")}
          >
            <h3>Spóźnione</h3>
            <p>{lateTasks}</p>
          </div>

          <div
            className={`stat-card done ${
              selectedFilter === "completed" ? "active" : ""
            }`}
            onClick={() => setSelectedFilter("completed")}
          >
            <h3>Wykonane</h3>
            <p>{completedTasks}</p>
          </div>

          <div
            className={`stat-card all ${
              selectedFilter === "all" ? "active" : ""
            }`}
            onClick={() => setSelectedFilter("all")}
          >
            <h3>Wszystkie zadania</h3>
            <p>{allTasks}</p>
          </div>
        </div>

        <div className="tasks-container">
          {filteredTasks.map((task) => (
            <TaskCard
              key={task.id}
              task={task}
              onComplete={toggleComplete}
              onEdit={handleEditTask}
              onDelete={toggleDelete}
            />
          ))}
        </div>
      </div>
      {loading && (
        <div className="loading-overlay">
          <div className="loading-box">
            <div className="spinner" />
            <p>Proszę czekać</p>
          </div>
        </div>
      )}
      <TaskModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSave={handleSaveTask}
        task={editingTask}
        loading={loading}
      />
    </>
  );
}

export default DashboardPage;
