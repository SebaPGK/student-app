import { render, screen, fireEvent } from "@testing-library/react";
import { vi } from "vitest";
import Header from "./Header";

describe("Header", () => {
  it("Shows username", () => {
    const user = {
      username: "Test User",
    };

    render(<Header user={user} onAddTask={vi.fn()} onLogout={vi.fn()} />);

    expect(screen.getByText(/Witaj, Test User/i)).toBeInTheDocument();
  });

  it("Calls onAddTask when new task is clicked", () => {
    const onAddTask = vi.fn();

    render(
      <Header
        user={{ displayName: "Test User" }}
        onAddTask={onAddTask}
        onLogout={vi.fn()}
      />,
    );

    fireEvent.click(
      screen.getByRole("button", {
        name: /\+ Dodaj zadanie/i,
      }),
    );

    expect(onAddTask).toHaveBeenCalledTimes(1);
  });

  it("Calls onLogout when logout button is clicked", () => {
    const onLogout = vi.fn();

    render(
      <Header
        user={{ displayName: "Test User" }}
        onAddTask={vi.fn()}
        onLogout={onLogout}
      />,
    );

    fireEvent.click(
      screen.getByRole("button", {
        name: /Wyloguj/i,
      }),
    );

    expect(onLogout).toHaveBeenCalledTimes(1);
  });
});
