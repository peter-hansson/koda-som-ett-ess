"""Tester för bokningssystemet."""

from datetime import datetime

import pytest

from src.booking import BookingSystem, BookingError


def test_book_a_court():
    system = BookingSystem()
    booking = system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 10, 0))
    assert booking.court == "Bana 1"
    assert booking.email == "anna@test.se"
    assert booking.start == datetime(2026, 3, 10, 10, 0)
    assert booking.end == datetime(2026, 3, 10, 11, 0)


def test_booking_has_unique_id():
    system = BookingSystem()
    b1 = system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 10, 0))
    b2 = system.book("Bana 2", "bob@test.se", datetime(2026, 3, 10, 10, 0))
    assert b1.id != b2.id


def test_no_double_booking():
    system = BookingSystem()
    system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 10, 0))
    with pytest.raises(BookingError, match="upptagen"):
        system.book("Bana 1", "bob@test.se", datetime(2026, 3, 10, 10, 0))


def test_peak_price():
    system = BookingSystem()
    booking = system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 17, 0))
    assert booking.price == 200


def test_off_peak_price():
    system = BookingSystem()
    booking = system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 10, 0))
    assert booking.price == 100


def test_cancel_booking():
    system = BookingSystem()
    booking = system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 10, 0))
    system.cancel(booking.id, current_time=datetime(2026, 3, 10, 7, 0))
    assert len(system.list_bookings()) == 0


def test_max_two_per_day():
    system = BookingSystem()
    system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 10, 0))
    system.book("Bana 2", "anna@test.se", datetime(2026, 3, 10, 14, 0))
    with pytest.raises(BookingError, match="max"):
        system.book("Bana 1", "anna@test.se", datetime(2026, 3, 10, 18, 0))
