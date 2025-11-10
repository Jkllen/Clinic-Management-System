# Clinic Management System
---

1. Team Git Workflow Guide
- Each member runs:
```bash
    git clone https://github.com/Jkllen/Clinic-Management-System
    cd Clinic-Management-System
```
---

2. Set Up Feature Branch
- Always start from **dev** branch:
```bash
    git checkout dev
    git pull origin dev
```
- Create your **feature branch** (replace **<feature-name>**);
```bash
    git checkout -b feature/<feature-name>
```
**Examples:**
- feature/login
- feature/add-patient
- feature/appointment

**DO NOT PUSH DIRECTLY TO MAIN - it's protected.**

---
3. Work on your Feature
- Add your code, implement UI, validation, etc.
- Commit changes often with clear messages:
```bash
    git add .
    git commit -m "Implemented the example."
```

---

4. Push Feature Branch to Remote
```bash
    git push origin feature/<feature-name>
```
---

5. Open a Pull Request
Go to GitHub → your repository → **Pull Requests** → **New Pull Request**

- Base branch: dev

- Compare branch: your feature/<feature-name> branch

- Add description: what the PR does, any notes

> A PR must be reviewed and approved before merging.

---

6. Merge PR
- Once approved, merged into **dev** branch.
- Never merge directly into **main** - all mearges go through **dev**.

---

7. Keep Branch Updated
**Before starting new work:**
```bash
    git checkout dev
    git pull origin dev
    git checkout feature/<new-feature>
    git merge dev
```
> This ensures your branch has the latest changes.

---

**NOTE:**
- **git checkout -b feature/feature-name** change in between feature directory.