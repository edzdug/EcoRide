import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormDataService } from '../itineraireform/itineraireform.component';
import { Time } from '@angular/common';
import { delay, Observable } from 'rxjs';

interface Covoiturage {
  id?: number;
  dateDepart: string;     // ou Date, si tu veux parser
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
  selector: 'app-itinerairevue',
  standalone: false,
  templateUrl: './itinerairevue.component.html',
  styleUrl: './itinerairevue.component.css'
})
export class ItinerairevueComponent implements OnInit {
  //private apiUrl = 'http://localhost:61576/api/covoiturage/GetItiniraireAll';
  form: any;
  public covoiturages: Covoiturage[] = [];
  list: any;
  constructor(private http: HttpClient, private formDataService: FormDataService) { }



  ngOnInit() {
    this.form = this.formDataService.getForm();
    
    this.getAll();    
  }

  getAll() {
    this.http.get<Covoiturage[]>('/api/Covoiturage/GetItiniraireAll')
    .subscribe(
      (data) => {
        // Sort by DateDepart descending
        this.covoiturages = data.sort((a, b) =>
          new Date(b.dateDepart).getTime() - new Date(a.dateDepart).getTime()
        );
        console.log("Données reçues : ", this.covoiturages);
        this.filterCovoiturages();
      },
      (error) => {
        console.error('Failed to load covoiturages:', error);
      }
    );
  }

  filteredCovoiturages: Covoiturage[] = [];

  filterCovoiturages(): void {
    if (!this.form) return;

    this.filteredCovoiturages = this.covoiturages.filter((covoiturage) => {
      const sameDepart = (covoiturage.lieuDepart || '').toLowerCase() === (this.form.depart || '').toLowerCase();
      const sameDestination = (covoiturage.lieuArrivee || '').toLowerCase() === (this.form.destination || '').toLowerCase();

      const covoiturageDate = new Date(covoiturage.dateDepart);
      const formDate = new Date(this.form.date);

      const sameDate =
        covoiturageDate.getFullYear() === formDate.getFullYear() &&
        covoiturageDate.getMonth() === formDate.getMonth() &&
        covoiturageDate.getDate() === formDate.getDate();

      return sameDepart && sameDestination && sameDate;
    });

    console.log(" Résultats filtrés :", this.filteredCovoiturages);
  }



}
