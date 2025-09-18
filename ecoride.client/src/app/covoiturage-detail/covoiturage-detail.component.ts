import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../authentification/auth.service';

interface AvisDto {
  note?: string;
  commentaire?: string;
}

interface CovoiturageDetail {
  covoiturage: {
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
    energie?: string;
  };
  marque: string;
  modele: string;
  preferenceConducteur: {
    fumeur: boolean;
    animal: boolean;
    autre?: string;
  };
  avisConducteur: AvisDto[];
}


@Component({
  selector: 'app-covoiturage-detail',
  standalone: false,
  templateUrl: './covoiturage-detail.component.html',
  styleUrl: './covoiturage-detail.component.css'
})
export class CovoiturageDetailComponent implements OnInit {
  covoiturageDetail?: CovoiturageDetail;

  constructor(private route: ActivatedRoute, private http: HttpClient, private router: Router, private authService: AuthService) { }

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      console.error('Id covoiturage manquant');
      return;
    }

    this.http.get<CovoiturageDetail>(`/api/Covoiturage/GetDetail/${id}`).subscribe({
      next: data => this.covoiturageDetail = data,
      error: err => console.error('Erreur récupération détail', err),
    });
  }

  confirmerParticipation() {
    if (!this.authService.isLoggedIn) {
      alert("Vous devez être connecté pour participer.");
      this.router.navigate(['/login']);
      return;
    }

    const user = this.authService.currentUserValue;
    const utilisateurId = user?.id; // ou user.utilisateur_id selon ton backend

    if (!utilisateurId) {
      alert("Erreur d'identification utilisateur.");
      return;
    }

    const prix = this.covoiturageDetail?.covoiturage.prixPersonne;
    const confirmMsg = `Ce trajet coûte ${prix} crédits. Confirmez-vous votre participation ?`;

    if (!confirm(confirmMsg)) return;

    const request = {
      utilisateurId,
      covoiturageId: this.covoiturageDetail?.covoiturage.id
    };

    this.http.post('/api/Covoiturage/Participer', request, { responseType: 'text' }).subscribe({
      next: msg => {
        alert(msg);
      },
      error: err => {
        alert("Erreur : " + JSON.stringify(err.error));
      }
    });
  }
}
