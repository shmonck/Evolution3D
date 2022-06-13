using System.Collections.Generic;
using UnityEngine;

public class Gene
{
    public string name;
    public float min, max;
    public float range;
    public float value;

    public Gene(string _name, float _min, float _max, float _range)
    {
        name = _name;
        min = _min;
        max = _max;
        range = _range;

        value = Random.Range(min, max);
    }

    public Gene Clone()
    {
        Gene g = new Gene(name, min, max, range);
        g.value = value;

        return g;
    }

    public void Mutate()
    {
        value += Random.Range(-range, range);

        if (value > max) value = max;
        else if (value < min) value = min;
    }
}

public class Genome
{
    public Color color;
    public List<Gene> genes = new List<Gene>();

    private void createGenome()
    {
        genes.Add(new Gene("Food Attraction", -1.0f, 1.0f, 0.2f));
        // genes.Add(new Gene("Poison Attraction", -1.0f, 1.0f, 0.2f));
        genes.Add(new Gene("Jump Cooldown", 0.8f, 3.5f, 0.3f));
        genes.Add(new Gene("Jump Force", 0.4f, 1.2f, 0.05f));
    }

    public Genome(Color _color)
    {
        color = _color;
    }

    public Genome()
    {
        color = new Color(Random.value, Random.value, Random.value);
        createGenome();
    }

    public Genome Clone()
    {
        Genome genome = new Genome(color);

        foreach(Gene gene in genes)
        {
            genome.genes.Add(gene.Clone());
        }

        return genome;
    }

    public void Mutate()
    {
        foreach(Gene gene in genes)
        {
            gene.Mutate();
        }

        color = new Color(Random.value, Random.value, Random.value);
    }

    public void Print()
    {
        string msg = "";

        foreach(Gene gene in genes)
        {
            msg += gene.name + " = " + gene.value + " ";
        }

        Debug.Log(msg);
    }

    public float this[string name]
    {
        get
        {
            foreach (Gene gene in genes)
                if (gene.name == name)
                    return gene.value;

            throw new System.Exception("Gene not found: " + name);
        }
    }
};

public class Creature : MonoBehaviour
{

    static float CREATURE_JUMP_FACTOR = 180.0f;
    static float CREATURE_HEALTH_LEAK = 0.25f;
    static float CREATURE_AGE_ADULT = 10.0f;

    public float health = 100.0f;
    public float age = 0.0f;
    public Genome genome;
    public Species species;

    float jumpTimer = 0.0f;

    public void AssignGenome(Genome _genome)
    {
        if (_genome == null)
            genome = new Genome();
        else
            genome = _genome.Clone();

        UpdateColor();
    }

    public void PrintGenome()
    {
        genome.Print();
    }

    GameObject GetClosestTrait()
    {
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        GameObject[] poisons = GameObject.FindGameObjectsWithTag("Poison");

        if (foods.Length == 0 && poisons.Length == 0)
            return null;

        GameObject closest;
        float closestDistance;

        if (foods.Length > 0)
            closest = foods[0];
        else
            closest = poisons[0];

        closestDistance = Vector3.Distance(transform.position, closest.transform.position);

        foreach(GameObject g in foods)
        {
            float distance = Vector3.Distance(transform.position, g.transform.position);

            if(distance < closestDistance)
            {
                closestDistance = distance;
                closest = g;
            }
        }

        foreach (GameObject g in poisons)
        {
            float distance = Vector3.Distance(transform.position, g.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = g;
            }
        }

        return closest;
    }

    public void Mutate()
    {
        genome.Mutate();
        UpdateColor();
    }

    void UpdateColor()
    {
        GetComponent<Renderer>().material.color = genome.color;
    }

    void Start()
    {
        
    }

    void Update()
    {
        health -= CREATURE_HEALTH_LEAK;

        if (health <= 0.0f)
        {
            Camera.main.GetComponent<Evolution>().ReportDeath(this);
            Destroy(gameObject);
            return;
        }

        age += Time.deltaTime;
        jumpTimer -= Time.deltaTime;

        if (age < CREATURE_AGE_ADULT)
        {
            float scale = 0.2f + age / CREATURE_AGE_ADULT * 0.3f;
            transform.localScale = new Vector3(scale, scale, scale);
        }
        else
        {
            if(transform.localScale.x < 0.5f)
            {
                transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }
        }

        if (jumpTimer > 0.0f)
            return;

        GameObject closest = GetClosestTrait();

        if (closest == null)
            return;

        Vector3 deltaUnit = (closest.transform.position - transform.position).normalized;

        if (closest != null)
        {
            if(closest.tag == "Food")
            {
                float x = genome["Food Attraction"];
                deltaUnit *= (0.5f * Mathf.Sign(x) + x * 0.5f);
            }
            /*
            else if(closest.tag == "Poison")
            {
                float x = genome["Poison Attraction"];
                deltaUnit *= (0.5f * Mathf.Sign(x) + x * 0.5f);
            }
            */
        }

        deltaUnit.y = 1.0f; 
        deltaUnit *= genome["Jump Force"] * CREATURE_JUMP_FACTOR;

        GetComponent<Rigidbody>().AddForce(deltaUnit);

        jumpTimer += genome["Jump Cooldown"];
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Food")
        {
            health += 50.0f;
            if(health > 100.0f)
            {
                if (age >= CREATURE_AGE_ADULT)
                    Camera.main.GetComponent<Evolution>().Reproduce(this);
                else
                    age += CREATURE_AGE_ADULT * 0.2f;

                if (health > 200.0f) health = 200.0f;
            }

            Destroy(collision.gameObject);
        }
        /*
        else if(collision.gameObject.tag == "Poison")
        {
            Camera.main.GetComponent<Evolution>().ReportDeath(this);
            Destroy(gameObject);
            Destroy(collision.gameObject);
        }
        */
    }
}
