"""Verifieringstester för workshopmaterialet.

Kör innan workshopen för att säkerställa att alla övningar,
filer och verktyg fungerar som förväntat.

    uv run --with pytest pytest tests/ -v
"""

import importlib
import subprocess
import sys
from pathlib import Path

import pytest

REPO_ROOT = Path(__file__).resolve().parent.parent
MATERIAL = REPO_ROOT / "material"


# === Övergripande struktur ===


class TestRepoStructure:
    """Verifiera att alla förväntade filer och mappar finns."""

    def test_exercise_dirs_exist(self):
        for d in ["1-fakturasystemet", "2-kontext-ar-kung", "3-tdd-med-ai", "4-ai-review"]:
            assert (REPO_ROOT / d).is_dir(), f"Saknar övningsmapp: {d}"

    def test_exercise_readmes_exist(self):
        for d in ["1-fakturasystemet", "2-kontext-ar-kung", "3-tdd-med-ai", "4-ai-review"]:
            assert (REPO_ROOT / d / "README.md").is_file(), f"Saknar README: {d}"

    def test_presenter_notes_exist(self):
        assert (REPO_ROOT / "presenter" / "anteckningar.md").is_file()

    def test_material_dir_exists(self):
        assert MATERIAL.is_dir()

    def test_github_action_exists(self):
        action = REPO_ROOT / ".github" / "workflows" / "ai-review.yml"
        assert action.is_file(), "Saknar GitHub Action: .github/workflows/ai-review.yml"


# === Övning 2: Kontext är kung ===


class TestExercise2:
    """Verifiera att kontextfiler och material för övning 2 finns."""

    def test_context_files_exist(self):
        material = MATERIAL / "ovning2-kontext"
        assert (material / "GEMINI.md").is_file()
        assert (material / "CLAUDE.md").is_file()
        assert (material / "cursor-rules").is_file()

    def test_context_files_have_content(self):
        material = MATERIAL / "ovning2-kontext"
        for name in ["GEMINI.md", "CLAUDE.md", "cursor-rules"]:
            content = (material / name).read_text()
            assert len(content) > 50, f"Kontextfilen {name} verkar tom"

    def test_context_files_identical_content(self):
        """Alla tre kontextfiler ska ha samma regler."""
        material = MATERIAL / "ovning2-kontext"
        gemini = (material / "GEMINI.md").read_text()
        claude = (material / "CLAUDE.md").read_text()
        cursor = (material / "cursor-rules").read_text()
        assert gemini == claude, "GEMINI.md och CLAUDE.md har olika innehåll"
        assert gemini == cursor, "GEMINI.md och cursor-rules har olika innehåll"


# === Övning 3: TDD med AI ===


class TestExercise3:
    """Verifiera att bokningssystemets testsvit och stubs fungerar."""

    EXERCISE3 = REPO_ROOT / "3-tdd-med-ai"

    def test_booking_stub_exists(self):
        assert (self.EXERCISE3 / "src" / "booking.py").is_file()

    def test_booking_ui_stub_exists(self):
        assert (self.EXERCISE3 / "src" / "booking_ui.py").is_file()

    def test_booking_tests_exist(self):
        assert (self.EXERCISE3 / "tests" / "test_booking.py").is_file()

    def test_ui_tests_exist(self):
        assert (self.EXERCISE3 / "tests" / "test_ui.py").is_file()

    def test_pyproject_exists(self):
        assert (self.EXERCISE3 / "pyproject.toml").is_file()

    def test_booking_stub_importable(self):
        """Stubs ska gå att importera utan fel."""
        sys.path.insert(0, str(self.EXERCISE3))
        try:
            import src.booking as mod
            importlib.reload(mod)
            assert hasattr(mod, "BookingSystem")
            assert hasattr(mod, "BookingError")
        finally:
            sys.path.pop(0)

    def test_booking_tests_discoverable(self):
        """pytest ska hitta testerna."""
        result = subprocess.run(
            [sys.executable, "-m", "pytest", "--collect-only", "-q", "tests/"],
            cwd=self.EXERCISE3,
            capture_output=True,
            text=True,
        )
        assert "test_book_a_court" in result.stdout, (
            f"Kan inte hitta tester. stderr: {result.stderr}"
        )

    def test_booking_tests_fail_with_stubs(self):
        """Alla tester ska misslyckas mot stubs (det är meningen)."""
        result = subprocess.run(
            [sys.executable, "-m", "pytest", "tests/test_booking.py", "-q"],
            cwd=self.EXERCISE3,
            capture_output=True,
            text=True,
        )
        assert result.returncode != 0, "Testerna passerade mot stubs — de ska misslyckas!"

    def test_ui_tests_discoverable(self):
        """pytest ska hitta UI-testerna."""
        result = subprocess.run(
            [sys.executable, "-m", "pytest", "--collect-only", "-q", "tests/test_ui.py"],
            cwd=self.EXERCISE3,
            capture_output=True,
            text=True,
        )
        assert "test_window_title" in result.stdout, (
            f"Kan inte hitta UI-tester. stderr: {result.stderr}"
        )

    def test_minimum_booking_test_count(self):
        """Det ska finnas minst 15 boknings-tester."""
        result = subprocess.run(
            [sys.executable, "-m", "pytest", "--collect-only", "-q", "tests/test_booking.py"],
            cwd=self.EXERCISE3,
            capture_output=True,
            text=True,
        )
        lines = [l for l in result.stdout.strip().splitlines() if "::test_" in l]
        assert len(lines) >= 15, f"Bara {len(lines)} tester hittades, förväntar minst 15"

    def test_minimum_ui_test_count(self):
        """Det ska finnas minst 10 UI-tester."""
        result = subprocess.run(
            [sys.executable, "-m", "pytest", "--collect-only", "-q", "tests/test_ui.py"],
            cwd=self.EXERCISE3,
            capture_output=True,
            text=True,
        )
        lines = [l for l in result.stdout.strip().splitlines() if "::test_" in l]
        assert len(lines) >= 10, f"Bara {len(lines)} UI-tester hittades, förväntar minst 10"


# === Övning 4: AI-review mot teamets kodstandard ===


class TestExercise4:
    """Verifiera att övning 4-materialet och GitHub Action finns."""

    EXERCISE4 = REPO_ROOT / "4-ai-review"

    def test_weather_app_exists(self):
        assert (self.EXERCISE4 / "src" / "weather_app.py").is_file()

    def test_weather_app_has_content(self):
        content = (self.EXERCISE4 / "src" / "weather_app.py").read_text()
        assert "def hämta_väder" in content
        assert "def visa_väder" in content

    def test_kodstandard_exists(self):
        path = MATERIAL / "ovning4-kodstandard" / "kodstandard.md"
        assert path.is_file(), "Saknar kodstandard.md"

    def test_kodstandard_has_sections(self):
        """Kodstandarden ska täcka de viktigaste områdena."""
        content = (MATERIAL / "ovning4-kodstandard" / "kodstandard.md").read_text()
        for section in ["Kodstil", "Verktyg", "Test", "Säkerhet", "Git"]:
            assert section in content, f"Kodstandarden saknar avsnitt: {section}"

    def test_github_action_references_kodstandard(self):
        action = (REPO_ROOT / ".github" / "workflows" / "ai-review.yml").read_text()
        assert "kodstandard.md" in action

    def test_github_action_triggers_on_exercise4(self):
        action = (REPO_ROOT / ".github" / "workflows" / "ai-review.yml").read_text()
        assert "4-ai-review/**" in action

    def test_github_action_filters_diff(self):
        """Action ska filtrera diffen till bara övning 4-filer."""
        action = (REPO_ROOT / ".github" / "workflows" / "ai-review.yml").read_text()
        assert "ALLOWED" in action
        assert "4-ai-review/" in action


# === Verktyg och beroenden ===


class TestTooling:
    """Verifiera att nödvändiga verktyg finns installerade."""

    def test_python_version(self):
        assert sys.version_info >= (3, 12), f"Kräver Python 3.12+, har {sys.version}"

    def test_pytest_available(self):
        result = subprocess.run(
            [sys.executable, "-m", "pytest", "--version"],
            capture_output=True,
            text=True,
        )
        assert result.returncode == 0

    def test_uv_available(self):
        result = subprocess.run(
            ["uv", "--version"],
            capture_output=True,
            text=True,
        )
        assert result.returncode == 0, "uv är inte installerat"

    def test_tkinter_available(self):
        """tkinter behövs för övning 3:s GUI-tester."""
        try:
            import tkinter
        except ImportError:
            pytest.fail("tkinter är inte installerat — behövs för övning 3")
