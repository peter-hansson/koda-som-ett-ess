"""Tester som simulerar kursdeltagarens flöde steg för steg.

Varje test motsvarar exakt det deltagaren förväntas göra i övningarna.
Referensimplementationer används istället för AI.

    uv run --with pytest pytest tests/test_participant_flow.py -v
"""

import importlib.util
import shutil
import subprocess
import sys
from pathlib import Path

import pytest

REPO_ROOT = Path(__file__).resolve().parent.parent
MATERIAL = REPO_ROOT / "material"


# === Övning 2: Kontext är kung ===


class TestExercise2Flow:
    """Simulerar deltagarens steg i övning 2."""

    def test_step1_create_empty_dir_and_work(self, tmp_path):
        """Steg 1: Deltagaren skapar en tom mapp (ingen kontextfil)."""
        work_dir = tmp_path / "utan-kontext"
        work_dir.mkdir()
        assert work_dir.is_dir()
        # Inga kontextfiler ska finnas
        assert not (work_dir / "CLAUDE.md").exists()
        assert not (work_dir / ".gemini").exists()

    def test_step2_copy_gemini_context(self, tmp_path):
        """Steg 2: Deltagaren kopierar GEMINI.md till rätt plats."""
        work_dir = tmp_path / "losenord-app"
        work_dir.mkdir()
        gemini_dir = work_dir / ".gemini"
        gemini_dir.mkdir()
        src = MATERIAL / "ovning2-kontext" / "GEMINI.md"
        dst = gemini_dir / "GEMINI.md"
        shutil.copy(src, dst)

        assert dst.is_file()
        content = dst.read_text()
        assert "Type hints" in content
        assert "Google-format" in content

    def test_step2_copy_claude_context(self, tmp_path):
        """Steg 2: Deltagaren kopierar CLAUDE.md till rätt plats."""
        work_dir = tmp_path / "losenord-app"
        work_dir.mkdir()
        src = MATERIAL / "ovning2-kontext" / "CLAUDE.md"
        dst = work_dir / "CLAUDE.md"
        shutil.copy(src, dst)

        assert dst.is_file()
        assert "Projektkonventioner" in dst.read_text()

    def test_step2_copy_cursor_context(self, tmp_path):
        """Steg 2: Deltagaren kopierar cursor-rules till rätt plats."""
        work_dir = tmp_path / "losenord-app"
        work_dir.mkdir()
        cursor_dir = work_dir / ".cursor"
        cursor_dir.mkdir()
        src = MATERIAL / "ovning2-kontext" / "cursor-rules"
        dst = cursor_dir / "rules"
        shutil.copy(src, dst)

        assert dst.is_file()
        assert "Projektkonventioner" in dst.read_text()

    def test_step4_extend_context_file(self, tmp_path):
        """Steg 4: Deltagaren lägger till en ny regel i kontextfilen."""
        work_dir = tmp_path / "losenord-app"
        work_dir.mkdir()
        ctx = work_dir / "CLAUDE.md"
        shutil.copy(MATERIAL / "ovning2-kontext" / "CLAUDE.md", ctx)

        original = ctx.read_text()
        new_rule = (
            "\n## Output-format\n"
            "Alla CLI-appar ska stödja både --json och --text output-format.\n"
            "Standardformat är text. JSON-output ska vara giltig JSON på stdout.\n"
        )
        ctx.write_text(original + new_rule)

        updated = ctx.read_text()
        assert "output-format" in updated.lower()
        assert "Projektkonventioner" in updated  # Originalinnehållet finns kvar


# === Övning 3: TDD med AI ===


class TestExercise3Flow:
    """Simulerar deltagarens steg i övning 3 med referensimplementation."""

    EXERCISE3 = REPO_ROOT / "3-tdd-med-ai"
    REFERENS = MATERIAL / "ovning3-referens"

    @pytest.fixture
    def exercise_copy(self, tmp_path):
        """Skapa en kopia av övning 3 att arbeta i."""
        dest = tmp_path / "3-tdd-med-ai"
        shutil.copytree(
            self.EXERCISE3, dest,
            ignore=shutil.ignore_patterns(".venv", "__pycache__", ".pytest_cache"),
        )
        return dest

    def test_step1_read_tests(self, exercise_copy):
        """Steg 1: Deltagaren läser testerna."""
        test_file = exercise_copy / "tests" / "test_booking.py"
        content = test_file.read_text()
        assert "test_book_a_court" in content
        assert "test_no_double_booking" in content
        assert "test_peak_price" in content

    def test_step2_all_tests_fail_with_stubs(self, exercise_copy):
        """Steg 2: Alla tester ska misslyckas mot stubs."""
        result = subprocess.run(
            [sys.executable, "-m", "pytest", "tests/test_booking.py", "-q"],
            cwd=exercise_copy,
            capture_output=True,
            text=True,
        )
        assert result.returncode != 0
        assert "failed" in result.stdout.lower() or "error" in result.stdout.lower()

    def test_step3_ai_implements_and_tests_pass(self, exercise_copy):
        """Steg 3: AI:n implementerar booking.py — alla tester gröna."""
        # Simulera AI:ns output med referensimplementation
        shutil.copy(self.REFERENS / "booking.py", exercise_copy / "src" / "booking.py")

        result = subprocess.run(
            [sys.executable, "-m", "pytest", "tests/test_booking.py", "-v"],
            cwd=exercise_copy,
            capture_output=True,
            text=True,
        )
        assert result.returncode == 0, f"Tester misslyckades:\n{result.stdout}\n{result.stderr}"

    def test_step4_ui_tests_fail_with_stubs(self, exercise_copy):
        """Steg 4a: UI-stuben saknar implementation."""
        stub = (exercise_copy / "src" / "booking_ui.py").read_text()
        assert "..." in stub or "pass" in stub
        assert "TODO" in stub or "Implementera" in stub

    def test_step4_reference_ui_creates_window(self):
        """Steg 4b: Referens-UI:t skapar ett fungerande fönster."""
        import tkinter as tk

        # Hoppa över om Tcl/Tk inte är tillgängligt (t.ex. uv:s managed Python)
        try:
            root = tk.Tk()
        except tk.TclError:
            pytest.skip("Tcl/Tk ej tillgängligt i denna Python-miljö")

        root.withdraw()

        # Importera referensimplementationen direkt
        sys.path.insert(0, str(self.EXERCISE3))
        sys.path.insert(0, str(self.REFERENS.parent))
        try:
            # Ladda referensens booking först
            spec = importlib.util.spec_from_file_location(
                "src.booking", self.REFERENS / "booking.py"
            )
            booking_mod = importlib.util.module_from_spec(spec)
            sys.modules["src.booking"] = booking_mod
            spec.loader.exec_module(booking_mod)

            # Ladda referensens UI
            spec = importlib.util.spec_from_file_location(
                "src.booking_ui", self.REFERENS / "booking_ui.py"
            )
            ui_mod = importlib.util.module_from_spec(spec)
            spec.loader.exec_module(ui_mod)

            try:
                app = ui_mod.BookingApp(root)
                root.update()

                # Verifiera att alla förväntade widgets finns
                assert app.court_entry.winfo_exists()
                assert app.email_entry.winfo_exists()
                assert app.time_entry.winfo_exists()
                assert app.book_button.winfo_exists()
                assert app.cancel_button.winfo_exists()

                # Testa en bokning
                app.court_entry.insert(0, "Bana 1")
                app.email_entry.insert(0, "anna@test.se")
                app.time_entry.insert(0, "2026-03-10 10:00")
                app.book_button.invoke()
                root.update()

                items = app.get_bookings_display()
                assert len(items) == 1
                assert "Bana 1" in items[0]
                assert "100" in items[0]
            finally:
                root.destroy()
        finally:
            sys.path.pop(0)
            sys.path.pop(0)
            sys.modules.pop("src.booking", None)
            sys.modules.pop("src.booking_ui", None)

    def test_step4_all_booking_tests_pass_with_reference(self, exercise_copy):
        """Steg 4c: Alla bokningstester passerar med referensimplementation."""
        shutil.copy(self.REFERENS / "booking.py", exercise_copy / "src" / "booking.py")

        result = subprocess.run(
            [sys.executable, "-m", "pytest", "tests/test_booking.py", "-v"],
            cwd=exercise_copy,
            capture_output=True,
            text=True,
            timeout=30,
        )
        assert result.returncode == 0, f"Tester misslyckades:\n{result.stdout}\n{result.stderr}"

    def test_step5_add_weekend_test(self, exercise_copy):
        """Steg 5: Deltagaren lägger till helgtest — det ska misslyckas."""
        shutil.copy(self.REFERENS / "booking.py", exercise_copy / "src" / "booking.py")

        test_file = exercise_copy / "tests" / "test_booking.py"
        original = test_file.read_text()
        new_test = '''

def test_weekend_surcharge():
    """Helger har 50% prispåslag"""
    system = BookingSystem()
    booking = system.book("Bana 1", "anna@test.se",
                          datetime(2026, 3, 14, 10, 0))  # lördag
    assert booking.price == 150  # 100 * 1.5
'''
        test_file.write_text(original + new_test)

        result = subprocess.run(
            [sys.executable, "-m", "pytest", "tests/test_booking.py", "-v",
             "-k", "test_weekend_surcharge"],
            cwd=exercise_copy,
            capture_output=True,
            text=True,
        )
        # Ska misslyckas — referensen saknar helgpris
        assert result.returncode != 0, "Helgtestet passerade — referensen ska inte ha helgpris"


# === Övning 4: AI-review mot teamets kodstandard ===


class TestExercise4Flow:
    """Simulerar deltagarens git-flöde i övning 4."""

    EXERCISE4 = REPO_ROOT / "4-ai-review"

    @pytest.fixture
    def git_repo(self, tmp_path):
        """Skapa ett git-repo som simulerar deltagarens fork."""
        repo = tmp_path / "koda-som-ett-ess"
        shutil.copytree(
            REPO_ROOT, repo,
            ignore=shutil.ignore_patterns(".venv", "__pycache__", ".pytest_cache", ".git"),
        )
        subprocess.run(["git", "init"], cwd=repo, capture_output=True)
        subprocess.run(["git", "add", "."], cwd=repo, capture_output=True)
        subprocess.run(
            ["git", "commit", "-m", "Initial"],
            cwd=repo, capture_output=True,
            env={**__import__("os").environ, "GIT_AUTHOR_NAME": "Test", "GIT_AUTHOR_EMAIL": "test@test.se",
                 "GIT_COMMITTER_NAME": "Test", "GIT_COMMITTER_EMAIL": "test@test.se"},
        )
        return repo

    def test_step1_create_branch(self, git_repo):
        """Steg 2: Deltagaren skapar en branch."""
        result = subprocess.run(
            ["git", "checkout", "-b", "deltagare/testsson/ovning4"],
            cwd=git_repo,
            capture_output=True,
            text=True,
        )
        assert result.returncode == 0

        result = subprocess.run(
            ["git", "branch", "--show-current"],
            cwd=git_repo,
            capture_output=True,
            text=True,
        )
        assert "deltagare/testsson/ovning4" in result.stdout

    def test_step2_alt_a_copy_weather_app_to_own_dir(self, git_repo):
        """Steg 2 Alt A: Deltagaren kopierar väderappen till sin mapp."""
        exercise4 = git_repo / "4-ai-review"
        user_dir = exercise4 / "testsson"
        user_dir.mkdir()
        shutil.copy(exercise4 / "src" / "weather_app.py", user_dir / "weather_app.py")

        assert (user_dir / "weather_app.py").is_file()
        assert "def hämta_väder" in (user_dir / "weather_app.py").read_text()

    def test_step2_alt_b_copy_booking_system_to_own_dir(self, git_repo):
        """Steg 2 Alt B: Deltagaren kopierar bokningssystemet till sin mapp."""
        exercise4 = git_repo / "4-ai-review"
        exercise3 = git_repo / "3-tdd-med-ai"
        user_dir = exercise4 / "testsson"
        user_dir.mkdir()

        shutil.copytree(exercise3 / "src", user_dir / "src")
        shutil.copytree(exercise3 / "tests", user_dir / "tests")

        assert (user_dir / "src" / "booking.py").is_file()
        assert (user_dir / "tests" / "test_booking.py").is_file()

    def test_step3_commit_changes_in_own_dir(self, git_repo):
        """Steg 3: Deltagaren committar ändringar i sin unika mapp."""
        env = {
            **__import__("os").environ,
            "GIT_AUTHOR_NAME": "Test",
            "GIT_AUTHOR_EMAIL": "test@test.se",
            "GIT_COMMITTER_NAME": "Test",
            "GIT_COMMITTER_EMAIL": "test@test.se",
        }

        subprocess.run(
            ["git", "checkout", "-b", "deltagare/testsson/ovning4"],
            cwd=git_repo, capture_output=True,
        )

        # Simulera en ändring i deltagarens unika mapp
        user_dir = git_repo / "4-ai-review" / "testsson"
        user_dir.mkdir()
        (user_dir / "my_solution.py").write_text("# Min lösning\nprint('hej')\n")

        subprocess.run(["git", "add", "."], cwd=git_repo, capture_output=True)
        result = subprocess.run(
            ["git", "commit", "-m", "Övning 4: Min lösning"],
            cwd=git_repo, capture_output=True, text=True, env=env,
        )
        assert result.returncode == 0

        # Verifiera att committen finns
        log = subprocess.run(
            ["git", "log", "--oneline", "-1"],
            cwd=git_repo, capture_output=True, text=True,
        )
        assert "Min lösning" in log.stdout

    def test_step5_kodstandard_accessible_for_review(self):
        """Steg 5: Kodstandarden är tillgänglig för AI-reviewern."""
        kodstandard = MATERIAL / "ovning4-kodstandard" / "kodstandard.md"
        assert kodstandard.is_file()
        content = kodstandard.read_text()
        # Verifiera att den har tillräckligt med regler att granska mot
        assert "ruff" in content.lower()
        assert "mypy" in content.lower()
        assert "Type hints" in content
        assert "Säkerhet" in content

    def test_github_action_would_filter_correctly(self):
        """Verifiera att diff-filtret i GitHub Action är korrekt konfigurerat."""
        action = (REPO_ROOT / ".github" / "workflows" / "ai-review.yml").read_text()

        # Ska bara triggas på övning 4-filer
        assert "4-ai-review/**" in action

        # Ska filtrera diffen
        assert "4-ai-review/" in action

        # Ska referera till kodstandarden
        assert "kodstandard.md" in action

        # Ska posta review med approve/request_changes
        assert "APPROVE" in action
        assert "REQUEST_CHANGES" in action
