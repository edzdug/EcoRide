import { Component, OnInit } from '@angular/core';
import { FormDataService } from '../itineraireform/itineraireform.component';
import { Time } from '@angular/common';

interface Covoiturage
{
          Id?: number;
          DateDepart: Date;
          heureDepart: Date;
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

  form: any;

  constructor(private formDataService: FormDataService) { }

  ngOnInit() {
    this.form = this.formDataService.getForm();
  }
}
