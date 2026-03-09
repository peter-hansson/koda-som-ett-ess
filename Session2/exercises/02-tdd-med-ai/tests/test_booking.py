"""Tester för padelbanebokningssystemet.

Dessa tester definierar affärsreglerna. Låt AI:n implementera
koden i src/booking.py så att alla tester passerar.
"""

from datetime import datetime

import pytest

from src.booking import BookingSystem, BookingError


# --- Grundläggande bokning ---


def test_book_a_court():
    """Kan boka en bana på en ledig tid."""
    system = BookingSystem()
    booking = system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 10, 0))

    assert booking.court == "Bana 1"
    assert booking.email == "anna@test.se"
    assert booking.start == datetime(2026, 3, 10, 10, 0)
    assert booking.end == datetime(2026, 3, 10, 11, 0)


def test_booking_has_unique_id():
    """Varje bokning får ett unikt ID."""
    system = BookingSystem()
    b1 = system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 10, 0))
    b2 = system.book("Bana 2", "bob@test.se", datetime(2026, 3, 10, 10, 0))

    assert b1.id != b2.id


def test_list_bookings():
    """Kan lista alla bokningar."""
    system = BookingSystem()
    system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 10, 0))
    system.book("Bana 2", "bob@test.se", datetime(2026, 3, 10, 14, 0))

    assert len(system.list_bookings()) == 2


# --- Tidsregler ---


def test_reject_booking_before_07():
    """Kan inte boka före 07:00."""
    system = BookingSystem()

    with pytest.raises(BookingError, match="07:00"):
        system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 6, 0))


def test_reject_booking_after_21():
    """Kan inte boka efter 21:00 (slutar 22:00)."""
    system = BookingSystem()

    with pytest.raises(BookingError, match="22:00"):
        system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 22, 0))


def test_booking_at_21_is_ok():
    """Bokning kl 21:00 är ok (slutar 22:00)."""
    system = BookingSystem()
    booking = system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 21, 0))

    assert booking.end == datetime(2026, 3, 10, 22, 0)


# --- Dubbelbokning ---


def test_no_double_booking():
    """Kan inte dubbelboka samma bana och tid."""
    system = BookingSystem()
    system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 10, 0))

    with pytest.raises(BookingError, match="upptagen"):
        system.book("Bana 1", "bob@test.se", datetime(2026, 3, 10, 10, 0))


def test_same_time_different_court_is_ok():
    """Samma tid på olika banor är ok."""
    system = BookingSystem()
    system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 10, 0))
    booking = system.book("Bana 2", "bob@test.se", datetime(2026, 3, 10, 10, 0))

    assert booking.court == "Bana 2"


# --- Max bokningar per person ---


def test_max_two_bookings_per_person_per_day():
    """Max 2 bokningar per person och dag."""
    system = BookingSystem()
    system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 10, 0))
    system.book("Bana 2", "anna@test.se", datetime(2026, 3, 10, 14, 0))

    with pytest.raises(BookingError, match="max"):
        system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 18, 0))


def test_different_days_resets_limit():
    """Begränsningen gäller per dag — ny dag, nya bokningar."""
    system = BookingSystem()
    system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 10, 0))
    system.book("Bana 2", "anna@test.se", datetime(2026, 3, 10, 14, 0))

    # Nästa dag — ska fungera
    booking = system.book("Bana 1", "anna@test.se", datetime(2026, 3, 11, 10, 0))
    assert booking.start.day == 11


# --- Avbokning ---


def test_cancel_booking():
    """Kan avboka en bokning."""
    system = BookingSystem()
    booking = system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 10, 0))

    system.cancel(booking.id, current_time=datetime(2026, 3, 10, 7, 0))

    assert len(system.list_bookings()) == 0


def test_cancel_must_be_2h_before():
    """Avbokning måste ske minst 2 timmar före starttid."""
    system = BookingSystem()
    booking = system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 10, 0))

    with pytest.raises(BookingError, match="2 timmar"):
        system.cancel(booking.id, current_time=datetime(2026, 3, 10, 9, 0))


def test_cancel_frees_slot():
    """Efter avbokning kan någon annan boka samma tid."""
    system = BookingSystem()
    booking = system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 10, 0))
    system.cancel(booking.id, current_time=datetime(2026, 3, 10, 7, 0))

    new_booking = system.book("Bana 1", "bob@test.se", datetime(2026, 3, 10, 10, 0))
    assert new_booking.email == "bob@test.se"


# --- Prissättning ---


def test_off_peak_price():
    """Pris utanför högtrafik är 100 kr."""
    system = BookingSystem()
    booking = system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 10, 0))

    assert booking.price == 100


def test_peak_price():
    """Pris under högtrafik (17:00-20:00) är 200 kr."""
    system = BookingSystem()
    booking = system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 17, 0))

    assert booking.price == 200


def test_peak_boundary_start():
    """Kl 17:00 är högtrafik."""
    system = BookingSystem()
    booking = system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 17, 0))

    assert booking.price == 200


def test_peak_boundary_end():
    """Kl 19:00 (slutar 20:00) är fortfarande högtrafik."""
    system = BookingSystem()
    booking = system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 19, 0))

    assert booking.price == 200


def test_after_peak():
    """Kl 20:00 är inte högtrafik längre."""
    system = BookingSystem()
    booking = system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 20, 0))

    assert booking.price == 100
