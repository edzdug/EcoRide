import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router'; // ⬅️ Pour récupérer l'ID depuis l'URL

interface Covoiturage {
  id: number;
  dateDepart: string;
  heureDepart: string;
  lieuDepart: string;
  dateArrivee: string;
  heureArrivee: string;
  lieuArrivee: string;
  statut?: string;
  nbPlace: number;
  prixPersonne: number;
}

@Component({
  selector: 'app-historique',
  standalone: false,
  templateUrl: './historique.component.html',
  styleUrl: './historique.component.css'
})
export class HistoriqueComponent {
  covoiturages: Covoiturage[] = [];
  utilisateurId!: number;
  isChauffeurMap: { [covoiturageId: number]: boolean } = {};

  constructor(private http: HttpClient, private route: ActivatedRoute) { }

  ngOnInit() {
    // Récupération de l'ID depuis l'URL
    this.utilisateurId = Number(this.route.snapshot.paramMap.get('id'));
    this.getAll();
  }

  getAll() {
    if (!this.utilisateurId) {
      console.error("Utilisateur ID manquant dans l'URL.");
      return;
    }

    this.http.get<Covoiturage[]>(`/api/Covoiturage/GetHistorique/${this.utilisateurId}`)
      .subscribe(
        (data) => {
          // Tri décroissant par date de départ
          this.covoiturages = data.sort((a, b) =>
            new Date(b.dateDepart).getTime() - new Date(a.dateDepart).getTime()
          );
          this.covoiturages.forEach(c => this.checkIsChauffeur(c.id!));
          console.log("Données reçues : ", this.covoiturages);
        },
        (error) => {
          console.error('Échec du chargement des covoiturages :', error);
        }
      );
  }

  annulerCovoiturage(covoiturageId: number | undefined) {
    if (!covoiturageId) return;

    const confirmer = confirm("Voulez-vous vraiment annuler ce covoiturage ?");
    if (!confirmer) return;

    this.http.delete(`/api/Covoiturage/Annuler/${covoiturageId}/utilisateur/${this.utilisateurId}`)
      .subscribe({
        next: () => {
          alert("Annulation réussie.");
          this.getAll(); // Recharger la liste
        },
        error: (err) => {
          console.error('Erreur lors de l’annulation :', err);
          alert("Erreur lors de l'annulation.");
        }
      });
  }

  demarrerCovoiturage(covoiturageId: number) {
    const covoiturage = this.covoiturages.find(c => c.id === covoiturageId);
    if (!covoiturage) return;

    this.http.post(`/api/Covoiturage/Demarrer`, covoiturage.id).subscribe({
      next: () => {
        console.log('Statut mis à jour : covoiturage démarré');
        covoiturage.statut = 'en_cours';
      },
      error: (err) => console.error('Erreur lors de la mise à jour', err)
    });
  }

  arriverCovoiturage(covoiturageId: number) {
    const covoiturage = this.covoiturages.find(c => c.id === covoiturageId);
    if (!covoiturage) return;

    this.http.post(`/api/Covoiturage/Arriver`, covoiturage.id).subscribe({
      next: () => {
        console.log('Statut mis à jour : covoiturage arrivé');
        covoiturage.statut = 'arrivé';
      },
      error: (err) => console.error('Erreur lors de la mise à jour', err)
    });
  }

  checkIsChauffeur(covoiturageId: number): void {
    this.http.get<{ isChauffeur: boolean }>(
      `/api/Covoiturage/isChauffeur/${covoiturageId}/utilisateur/${this.utilisateurId}`
    ).subscribe({
      next: (res) => {
        this.isChauffeurMap[covoiturageId] = res.isChauffeur;
      },
      error: (err) => {
        console.error(`Erreur lors de la vérification chauffeur pour ${covoiturageId}`, err);
        this.isChauffeurMap[covoiturageId] = false;
      }
    });
  }


}
