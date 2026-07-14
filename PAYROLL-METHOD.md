# Méthode de calcul des retenues à la source

OpenPayrun utilise la **méthode des formules** pour le calcul de la retenue d'impôt sur le revenu — T4127 (fédéral) et TP-1015.F (impôt provincial du Québec). Ces deux méthodes sont officiellement reconnues comme alternatives à la méthode des tables (T4032 / TP-1015.TI).

## Pourquoi les résultats diffèrent des logiciels utilisant les tables

La méthode des formules et la méthode des tables sont toutes deux acceptées, mais ne produiront pas des résultats identiques. La méthode des tables pré-discrétise les revenus en tranches et utilise des approximations à chaque étape ; la méthode des formules maintient une précision complète jusqu'au résultat final.

### Fédéral — T4127, janvier 2025 (chapitre 4, « Tax Deductions Comparison »)

> "When the tax deductions amount using Option 1 in this guide is compared to the tax deductions amount in the publication T4032, Payroll Deductions Tables, the amounts will not necessarily be the same. Any difference results from the fact that the amounts in the T4032 are based on:
> (i) the mid-point of the range of remuneration under the "Pay" column;
> (ii) the federal tax credit for Canada Pension Plan or Quebec Pension Plan contributions and employment insurance premium deductions is based on the amount determined in item (i);
> (iii) the mid-point of the "Claim code" amounts on federal, provincial, and territorial TD1 forms is used, except for code 1 where the actual basic personal non-refundable tax credit amount is used. For claim code 0, no personal tax credits amounts are used when calculating the tax deduction amounts."

Source : `backend/references/2025/T4127-jan2025-en.pdf`

### Québec — TP-1015.F, janvier 2025 (section 2.1, NOTE)

> « Si vous utilisez les formules pour le calcul de la retenue d'impôt sur la base de paiements réguliers, le montant obtenu ne sera pas nécessairement identique à celui qui figure dans le document Table des retenues à la source d'impôt du Québec (TP-1015.TI). Le fait que les retenues d'impôt n'ont pas la même base de calcul explique cette différence. »

Source : `backend/references/2025/TP-1015.F-2025-01-fr.pdf`

## Documents de référence

| Document | Autorité | Langue | Chemin |
|----------|----------|--------|--------|
| T4127 — Formules pour le calcul des retenues sur la paie, jan. 2025 | ARC | EN | `backend/references/2025/T4127-jan2025-en.pdf` |
| TP-1015.F — Formules pour le calcul des retenues à la source, jan. 2025 | Revenu Québec | FR | `backend/references/2025/TP-1015.F-2025-01-fr.pdf` |
