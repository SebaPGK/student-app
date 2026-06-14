import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { vi } from "vitest";
import { MemoryRouter } from "react-router-dom";
import AuthPage from "./AuthPage";

const navigateMock = vi.fn();

vi.mock("react-router-dom", async () => {
  const actual = await vi.importActual("react-router-dom");

  return {
    ...actual,
    useNavigate: () => navigateMock,
  };
});

describe("AuthPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();

    global.fetch = vi.fn();
    localStorage.clear();
  });

  it("changes form after clicking register button", () => {
    render(
      <MemoryRouter>
        <AuthPage />
      </MemoryRouter>
    );

    fireEvent.click(
      screen.getByRole("button", {
        name: /zarejestruj się/i,
      })
    );

    expect(
      screen.getByLabelText(/imię i nazwisko/i)
    ).toBeInTheDocument();

    expect(
      screen.getByLabelText(/^email$/i)
    ).toBeInTheDocument();

    expect(
      screen.getByLabelText(/^hasło$/i)
    ).toBeInTheDocument();

    expect(
      screen.getByLabelText(/potwierdź hasło/i)
    ).toBeInTheDocument();
  });

  it("logs user in and navigates to dashboard", async () => {
    fetch.mockResolvedValue({
      ok: true,
      text: () =>
        Promise.resolve(
          JSON.stringify({
            token: "jwt-token",
            user: {
              id: "123",
              email: "test@example.com",
            },
          })
        ),
    });

    render(
      <MemoryRouter>
        <AuthPage />
      </MemoryRouter>
    );

    fireEvent.change(screen.getByLabelText(/^email$/i), {
      target: { value: "test@example.com" },
    });

    fireEvent.change(screen.getByLabelText(/^hasło$/i), {
      target: { value: "password123" },
    });

    fireEvent.click(
      screen.getByRole("button", {
        name: /^zaloguj się$/i,
      })
    );

    await waitFor(() => {
      expect(fetch).toHaveBeenCalled();
    });

    expect(localStorage.getItem("token")).toBe("jwt-token");
    expect(navigateMock).toHaveBeenCalledWith("/dashboard");
  });

  it("registers user and switches back to login mode", async () => {
    fetch.mockResolvedValue({
      ok: true,
      text: () => Promise.resolve("{}"),
    });

    render(
      <MemoryRouter>
        <AuthPage />
      </MemoryRouter>
    );

    fireEvent.click(
      screen.getByRole("button", {
        name: /zarejestruj się/i,
      })
    );

    fireEvent.change(screen.getByLabelText(/imię i nazwisko/i), {
      target: { value: "Jan Kowalski" },
    });

    fireEvent.change(screen.getByLabelText(/^email$/i), {
      target: { value: "jan@test.pl" },
    });

    fireEvent.change(screen.getByLabelText(/^hasło$/i), {
      target: { value: "password123" },
    });

    fireEvent.change(screen.getByLabelText(/potwierdź hasło/i), {
      target: { value: "password123" },
    });

    fireEvent.click(
      screen.getByRole("button", {
        name: /utwórz konto/i,
      })
    );

    await waitFor(() => {
      expect(fetch).toHaveBeenCalled();
    });

    expect(
      screen.queryByLabelText(/potwierdź hasło/i)
    ).not.toBeInTheDocument();
  });
});