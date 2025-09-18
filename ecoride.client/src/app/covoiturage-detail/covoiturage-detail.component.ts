import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';

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

  constructor(private route: ActivatedRoute, private http: HttpClient) { }

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
}
