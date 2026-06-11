const API_URL = "http://localhost:7071/api";

export const getTasks = async () => {
  const url = API_URL + "/tasks";
  const token = localStorage.getItem("token");

  const res = await fetch(url, {
    method: "GET",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
  });
  if (!res.ok) {
    throw new Error("Cannot get task from database");
  }
  return await res.json();
};

export const createTask = async (taskData) => {
  const url = API_URL + "/tasks";
  const token = localStorage.getItem("token");

  const dto = {
    title: taskData.title,
    description: taskData.description,
    dueDate: taskData.date,
    priority: taskData.priority,
  };

  const res = await fetch(url, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(dto),
  });
  if (!res.ok) {
    throw new Error("Failed to create task");
  }
  return await res.json();
};

export const deleteTaskById = async (id) => {
  const url = `${API_URL}/tasks/${id}`;
  const token = localStorage.getItem("token");

  const res = await fetch(url, {
    method: "DELETE",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
  });
  if (!res.ok) {
    throw new Error("Failed to create task");
  }
};

export const updateTask = async (taskData) => {
  const url = `${API_URL}/tasks/${taskData.id}`;
  const token = localStorage.getItem("token");

  const dto = {
    title: taskData.title,
    description: taskData.description,
    dueDate: taskData.date,
    priority: taskData.priority,
  };

  const res = await fetch(url, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(dto),
  });
  if (!res.ok) {
    throw new Error("Failed to update task");
  }
};

export const loginUser = async (email, password) => {
  return {
    id: 1,
    firstName: "Jan",
    lastName: "Nowak",
    email,
  };
};

export const registerUser = async (userData) => {
  return {
    success: true,
  };
};
