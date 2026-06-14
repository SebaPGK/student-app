import { render, screen, fireEvent } from "@testing-library/react";
import { vi } from "vitest";
import TaskCard from "./TaskCard";

describe("TaskCard", () => {
  it("Call onComplete after clickin complete button", () => {
    const onComplete = vi.fn();

    render(
      <TaskCard
        task={{
          id: 1,
          title: "Test",
          priority: 2,
          description: "Description",
          dueDate: "2026-06-10",
          completed: false,
        }}
        onComplete={onComplete}
        onEdit={vi.fn()}
        onDelete={vi.fn()}
      />,
    );

    fireEvent.click(
      screen.getByRole("button", { name: /Oznacz jako wykonane/i }),
    );

    expect(onComplete).toHaveBeenCalledWith(1);
  });

  it("Call onEdti after clickin update button", () => {
    const onEdit = vi.fn();

    render(
      <TaskCard
        task={{
          id: 1,
          title: "Test",
          priority: 2,
          description: "Description",
          dueDate: "2026-06-10",
          completed: false,
        }}
        onComplete={vi.fn()}
        onEdit={onEdit}
        onDelete={vi.fn()}
      />,
    );

    fireEvent.click(screen.getByRole("button", { name: /Edytuj/i }));

    expect(onEdit).toHaveBeenCalledWith(1);
  });

  it("Call onDelete after clickin update button", () => {
    const onDelete = vi.fn();

    render(
      <TaskCard
        task={{
          id: 1,
          title: "Test",
          priority: 2,
          description: "Description",
          dueDate: "2026-06-10",
          completed: false,
        }}
        onComplete={vi.fn()}
        onEdit={vi.fn()}
        onDelete={onDelete}
      />,
    );

    fireEvent.click(screen.getByRole("button", { name: /Usuń/i }));

    expect(onDelete).toHaveBeenCalledWith(1);
  });
});
