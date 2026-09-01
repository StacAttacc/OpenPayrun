> 🇬🇧 [English version below](#scsi-tax-calculator-1)

---

# SCSI Tax Calculator

Calculateur de retenues à la source pour la paie québécoise — RRQ, AE, RQAP, impôt fédéral et provincial.

Disponible en français et en anglais. Gratuit.

## Ce que ça calcule

À partir d'un salaire brut et d'une période de paie, SCSI Tax Calculator calcule :

**Déductions employé**
- RRQ (palier 1 & 2)
- Prime AE
- Prime RQAP
- Impôt fédéral sur le revenu
- Impôt provincial du Québec
- Salaire net

**Cotisations employeur**
- RRQ (palier 1 & 2)
- AE
- RQAP
- FSS

## Méthode de calcul

Les calculs suivent la **méthode des formules** (T4127 pour le fédéral, TP-1015.F pour le Québec) — l'alternative aux tables de retenues reconnue par l'ARC et Revenu Québec. Les résultats peuvent différer légèrement des logiciels utilisant les tables ; voir [PAYROLL-METHOD.md](PAYROLL-METHOD.md) pour les détails.

## Guide développeur

### Prérequis

- Docker + Docker Compose
- .NET 9 (backend)
- Node 22 + Angular CLI (frontend)

### Démarrage local

```bash
docker compose up
```

Frontend : http://localhost:4200  
API backend : http://localhost:5038

Le frontend recharge automatiquement les changements de source. Le backend nécessite un rebuild.

### Stack

| Couche | Technologie |
|--------|-------------|
| Frontend | Angular 22, TailwindCSS v4, DaisyUI v5, TanStack Query |
| Backend | ASP.NET Core 9, Entity Framework Core, SQL Server |
| Infrastructure | Docker Compose (dev), Kubernetes + Flux (prod) |

### Accès administrateur

Les jeux de taux sont gérés depuis l'interface. Connectez-vous avec les identifiants admin pour ajouter, modifier ou supprimer des jeux de taux. Sans connexion, la calculatrice et le tableau des taux sont en lecture seule.

### Déploiement

L'application est déployée via Flux GitOps. Un push sur `main` déclenche un build d'image ; Flux détecte le nouveau tag et met à jour le cluster automatiquement.

---

MIT License © 2026 StacAttacc

---

# SCSI Tax Calculator

Quebec payroll deductions calculator — QPP/RRQ, EI/AE, QPIP/RQAP, federal and provincial income tax.

Available in English and French. Free to use.

## What it calculates

Given a gross pay amount and pay period, SCSI Tax Calculator computes:

**Employee deductions**
- QPP (Tier 1 & 2)
- EI premium
- QPIP premium
- Federal income tax
- Quebec income tax
- Net pay

**Employer contributions**
- QPP (Tier 1 & 2)
- EI
- QPIP
- FSSQ

## Method

Calculations follow the **formula method** (T4127 for federal, TP-1015.F for Quebec) — the CRA- and Revenu Québec-approved alternative to the payroll tables. Results may differ slightly from table-based software; see [PAYROLL-METHOD.md](PAYROLL-METHOD.md) for details.

## Developer guide

### Requirements

- Docker + Docker Compose
- .NET 9 (backend)
- Node 22 + Angular CLI (frontend)

### Run locally

```bash
docker compose up
```

Frontend: http://localhost:4200  
Backend API: http://localhost:5038

The frontend hot-reloads on source changes. The backend requires a rebuild on changes.

### Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Angular 22, TailwindCSS v4, DaisyUI v5, TanStack Query |
| Backend | ASP.NET Core 9, Entity Framework Core, SQL Server |
| Infrastructure | Docker Compose (dev), Kubernetes + Flux (prod) |

### Admin access

Tax rate sets are managed through the UI. Log in with admin credentials to add, edit, or delete rate sets. Without login, the calculator and rate table are read-only.

### Production deployment

The app is deployed via Flux GitOps. Pushing to `main` triggers an image build; Flux detects the new tag and updates the cluster automatically.

---

MIT License © 2026 StacAttacc
