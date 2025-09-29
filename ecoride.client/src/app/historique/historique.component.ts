import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router'; // ⬅️ Pour récupérer l'ID depuis l'URL

interface Covoiturage {
  id?: number;
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

}
