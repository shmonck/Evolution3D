using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class Species
{
    public Genome representative;
    List<Creature> creatures = new List<Creature>();

    public Species(Creature initCreature)
    {
        if (initCreature == null)
        {
            representative = new Genome();
        }
        else
        {
            representative = initCreature.genome.Clone();
            AddCreature(initCreature);
        }
    }

    public int Size()
    {
        return creatures.Count;
    }

    public void AddCreature(Creature creature)
    {
        creature.species = this;
        creatures.Add(creature);
    }

    public void RemoveCreature(Creature creature)
    {
        creatures.Remove(creature);
    }
}

public class Evolution : MonoBehaviour
{

    public GameObject creaturePrefab;
    public GameObject foodPrefab;
    public GameObject poisonPrefab;
    public GameObject creatureShowcasePrefab;

    GameObject creatureShowcase;

    public TextMeshProUGUI text;
    public TextMeshProUGUI text2;

    const int INIT_SPECIE_SIZE = 5;
    const int INIT_SPECIE_COUNT = 5;
    const int INIT_FOOD_COUNT = 50;
    // const int INIT_POISON_COUNT = 10;

    const float GROUND_SIZE = 30.0f;

    const float FOOD_SPAWN_EVERY = 0.25f;
    // const float POISON_SPAWN_EVERY = 1.0f;

    const float CREATURE_MUTATE_CHANCE = 0.1f;

    const float CREATURE_SHOWCASE_ROTATE_SPEED = 30.0f;

    float foodTimer = 0.0f;
    // float poisonTimer = 0.0f;

    List<Species> allSpecies = new List<Species>();

    public void Reproduce(Creature creature)
    {
        GameObject go = Instantiate(creaturePrefab, creature.transform.position, Quaternion.identity);
        go.GetComponent<Creature>().AssignGenome(creature.genome);

        if (CREATURE_MUTATE_CHANCE >= Random.value)
        {
            go.GetComponent<Creature>().Mutate();
            go.GetComponent<Creature>().PrintGenome();
            Species species = new Species(go.GetComponent<Creature>());
            this.allSpecies.Add(species);
        }
        else
        {
            creature.species.AddCreature(go.GetComponent<Creature>());
        }

        UpdateText();
    }

    public void ReportDeath(Creature creature)
    {
        creature.species.RemoveCreature(creature);
        if (creature.species.Size() == 0)
        {
            allSpecies.Remove(creature.species);
        }

        UpdateText();
    }

    void Start()
    {

        for (int i = 0; i < INIT_SPECIE_COUNT; i++)
        {
            Species species = new Species(null);

            for (int j = 0; j < INIT_SPECIE_SIZE; j++)
            {
                GameObject go = Instantiate(creaturePrefab, new Vector3(Random.Range(-GROUND_SIZE / 2.0f, GROUND_SIZE / 2.0f), 0.4f, Random.Range(-GROUND_SIZE / 2.0f, GROUND_SIZE / 2.0f)), Quaternion.identity);
                go.GetComponent<Creature>().AssignGenome(species.representative);
                species.AddCreature(go.GetComponent<Creature>());
            }

            this.allSpecies.Add(species);
        }

        for (int i = 0; i < INIT_FOOD_COUNT; i++) SpawnFood();

        // for (int i = 0; i < INIT_POISON_COUNT; i++) SpawnPoison();

        creatureShowcase = Instantiate(creatureShowcasePrefab);

        UpdateText();
    }

    void Update()
    {
        if (creatureShowcase != null)
            creatureShowcase.transform.Rotate(Vector3.up * CREATURE_SHOWCASE_ROTATE_SPEED * Time.deltaTime);

        foodTimer += Time.deltaTime;
        // poisonTimer += Time.deltaTime;

        if (foodTimer >= FOOD_SPAWN_EVERY)
        {
            SpawnFood();

            foodTimer = 0.0f;
        }

        /*
        if (poisonTimer >= POISON_SPAWN_EVERY)
        {
            SpawnPoison();

            poisonTimer = 0.0f;
        }
        */
    }

    private void SpawnFood()
    {
        Instantiate(foodPrefab, new Vector3(Random.Range(-GROUND_SIZE / 2.0f, GROUND_SIZE / 2.0f), 0.25f, Random.Range(-GROUND_SIZE / 2.0f, GROUND_SIZE / 2.0f)), Quaternion.identity);
        UpdateText();
    }

    private void SpawnPoison()
    {
        Instantiate(poisonPrefab, new Vector3(Random.Range(-GROUND_SIZE / 2.0f, GROUND_SIZE / 2.0f), 0.25f, Random.Range(-GROUND_SIZE / 2.0f, GROUND_SIZE / 2.0f)), Quaternion.identity);
        UpdateText();
    }

    private void UpdateText()
    {
        Species best = null;

        foreach (Species species in allSpecies)
        {
            if (best == null)
            {
                best = species;
            }
            else
            {
                if (species.Size() > best.Size())
                    best = species;
            }
        }

        int totalPopulationSize = 0;
        foreach (Species species in allSpecies) totalPopulationSize += species.Size();

        string str = "Population size: " + totalPopulationSize + "\n";
        str += "Species count: " + allSpecies.Count + "\n";
        str += "Food count: " + GameObject.FindGameObjectsWithTag("Food").Length + "\n";
        str += "Poison count: " + GameObject.FindGameObjectsWithTag("Poison").Length;

        if (creatureShowcase != null)
        {
            if (best == null)
            {
                creatureShowcase.GetComponent<Renderer>().enabled = false;
            }
            else
            {
                creatureShowcase.GetComponent<Renderer>().enabled = true;
                creatureShowcase.GetComponent<Renderer>().material.color = best.representative.color;
            }
        }

        string str2 = "";

        if (best != null)
        {
            str2 += "BEST GENOME\n";

            foreach (Gene gene in best.representative.genes)
            {
                str2 += gene.name + " = " + gene.value + "\n";
            }
        }

        text2.SetText(str2);

        text.SetText(str);
    }
}
