const API_URL = "http://localhost:5174/api";

export const getTasks = async () => {
  const response = await fetch(API_URL);

  if (!response.ok) {
    throw new Error("Cannot get task from database");
  }
  return [
    {
      id: 1,
      title: "Zrobić projekt React",
      date: "2026-05-12",
      description: "Dokończyć dashboard aplikacji To-Do List.",
      completed: false,
    },
    {
      id: 2,
      title: "Nauka CSS",
      date: "2026-05-15",
      description: "Powtórzyć flexbox oraz grid.",
      completed: true,
    },
    {
      id: 3,
      title: "Oddać dokumentację",
      date: "2026-05-01",
      description: "Wysłać dokumentację projektu.",
      completed: false,
    },
    {
      id: 4,
      title: "Nowe zadanie",
      date: "2026-05-01",
      description: "Coś tam coś tam",
      completed: true,
    },
    {
      id: 5,
      title: "Nowe zadanie",
      date: "2026-05-01",
      description:
        "Coś tam coś tam Coś tam coś tamCoś tam coś tam vCoś tam coś tamCoś tam coś tam  Coś tam coś tamCoś tam coś tam ",
      completed: false,
    },
    {
      id: 6,
      title: "Nowe zadanie",
      date: "2026-05-01",
      description: "Coś tam coś tam",
      completed: false,
    },
    {
      id: 7,
      title: "Nowe zadanie",
      date: "2026-05-01",
      description: "Coś tam coś tam",
      completed: false,
    },
  ];
  //   return response.json();
};

export const createTask = async (taskData) => {
  const url = API_URL + "/tasks";
  const token = localStorage.getItem("token");
  console.log(token);

  const dto = {
    title: taskData.title,
    description: taskData.description,
    dueDate: taskData.date,
    // TODO: dodać priority do formularza
    priority: 1,
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
  return "OK";
};

export const updateTask = async (task) => {
  return "OK";
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
