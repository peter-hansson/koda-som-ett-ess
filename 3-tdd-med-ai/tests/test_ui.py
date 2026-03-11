"""UI-tester för bokningssystemets tkinter-gränssnitt.

Testerna definierar hur gränssnittet ska se ut och bete sig.
Låt AI:n implementera src/booking_ui.py så att alla tester passerar.
"""

import tkinter as tk

import pytest

from src.booking_ui import BookingApp


@pytest.fixture
def app():
    """Starta appen och stäng den efter varje test."""
    root = tk.Tk()
    booking_app = BookingApp(root)
    root.update()
    yield booking_app
    root.destroy()


# --- Fönster och layout ---


def test_window_title(app):
    """Fönstret ska ha en titel som nämner bokning."""
    title = app.root.title().lower()
    assert "bokning" in title or "padel" in title


def test_has_input_fields(app):
    """Appen ska ha inmatningsfält för bana, e-post och tid."""
    assert app.court_entry.winfo_exists()
    assert app.email_entry.winfo_exists()
    assert app.time_entry.winfo_exists()


def test_has_book_button(app):
    """Appen ska ha en boka-knapp."""
    assert app.book_button.winfo_exists()


def test_has_cancel_button(app):
    """Appen ska ha en avboka-knapp."""
    assert app.cancel_button.winfo_exists()


# --- Boka via GUI ---


def _fill_and_book(app, court="Bana 1", email="anna@test.se", time="2026-03-10 10:00"):
    """Hjälpfunktion: fyll i fälten och klicka boka."""
    app.court_entry.delete(0, tk.END)
    app.email_entry.delete(0, tk.END)
    app.time_entry.delete(0, tk.END)
    app.court_entry.insert(0, court)
    app.email_entry.insert(0, email)
    app.time_entry.insert(0, time)
    app.book_button.invoke()
    app.root.update()


def test_booking_appears_in_list(app):
    """En bokning ska synas i listan efter att man klickat boka."""
    _fill_and_book(app, "Bana 1", "anna@test.se", "2026-03-10 10:00")

    items = app.get_bookings_display()
    assert len(items) == 1
    assert "Bana 1" in items[0]
    assert "anna@test.se" in items[0]


def test_price_shown_in_list(app):
    """Priset ska visas i bokningslistan."""
    _fill_and_book(app, "Bana 1", "anna@test.se", "2026-03-10 10:00")

    items = app.get_bookings_display()
    assert "100" in items[0]


def test_peak_price_shown(app):
    """Högtrafik-pris (200 kr) ska visas för bokning kl 17-20."""
    _fill_and_book(app, "Bana 1", "anna@test.se", "2026-03-10 17:00")

    items = app.get_bookings_display()
    assert "200" in items[0]


def test_multiple_bookings_in_list(app):
    """Flera bokningar ska synas i listan."""
    _fill_and_book(app, "Bana 1", "anna@test.se", "2026-03-10 10:00")
    _fill_and_book(app, "Bana 2", "bob@test.se", "2026-03-10 14:00")

    items = app.get_bookings_display()
    assert len(items) == 2


def test_fields_cleared_after_booking(app):
    """Inmatningsfälten ska tömmas efter lyckad bokning."""
    _fill_and_book(app, "Bana 1", "anna@test.se", "2026-03-10 10:00")

    assert app.court_entry.get() == ""
    assert app.email_entry.get() == ""
    assert app.time_entry.get() == ""


# --- Felhantering i GUI ---


def test_error_on_double_booking(app):
    """Felmeddelande ska visas vid dubbelbokning."""
    _fill_and_book(app, "Bana 1", "anna@test.se", "2026-03-10 10:00")
    _fill_and_book(app, "Bana 1", "bob@test.se", "2026-03-10 10:00")

    error = app.get_status_message()
    assert "upptagen" in error.lower()


def test_error_on_invalid_time(app):
    """Felmeddelande vid ogiltig tid."""
    _fill_and_book(app, "Bana 1", "anna@test.se", "inte-en-tid")

    error = app.get_status_message()
    assert len(error) > 0  # Något felmeddelande ska visas


def test_success_message_on_booking(app):
    """Bekräftelsemeddelande ska visas vid lyckad bokning."""
    _fill_and_book(app, "Bana 1", "anna@test.se", "2026-03-10 10:00")

    status = app.get_status_message()
    assert len(status) > 0
    # Ska inte vara ett felmeddelande
    assert "fel" not in status.lower()
    assert "error" not in status.lower()


# --- Avboka via GUI ---


def test_cancel_removes_from_list(app):
    """Avbokad bokning ska försvinna från listan."""
    _fill_and_book(app, "Bana 1", "anna@test.se", "2026-12-10 10:00")

    app.select_booking(0)
    app.cancel_button.invoke()
    app.root.update()

    items = app.get_bookings_display()
    assert len(items) == 0
