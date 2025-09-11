import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormDataService } from '../itineraireform/itineraireform.component';
import { Time } from '@angular/common';
import { Observable } from 'rxjs';

interface Covoiturage
{
          Id?: number;
          DateDepart: Date;
          heureDepart: Time;
          LieuDepart: string;
          DateArrivee: Date;
          heureArrivee: string;
          LieuArrivee: string;
          Statut?: string;
          NbPlace: number;
          PrixPersonne: number;
}

@Component({
  selector: 'app-itinerairevue',
  standalone: false,
  templateUrl: './itinerairevue.component.html',
  styleUrl: './itinerairevue.component.css'
})
export class ItinerairevueComponent implements OnInit {
  private apiUrl = 'http://localhost:61576/api/covoiturage/GetItiniraireAll';
  form: any;
  public covoiturages: Covoiturage[] = [];
  list: any;
  constructor(private http: HttpClient, private formDataService: FormDataService) { }



  ngOnInit() {
    this.form = this.formDataService.getForm();
    this.getAll();
  }

  getAll(): Observable<any[]> {
    //this.list = this.http.get<any[]>(this.apiUrl);

    this.http.get<Covoiturage[]>('/api/Covoiturage/GetItiniraireAll').subscribe(
      (result) => {
        // Trie la liste par date (décroissant)
        this.covoiturages = result;
      },
      (error) => {
        console.error(error);
      }
    );

    return this.list;
  }
}
