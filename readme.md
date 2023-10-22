## Calculating SIL failure rates

This repository provides simple interface for specifying different schemes and calculating their failure rate.

### Algorithm

The algorithm assumes all possible element failures - independed and common-cause failures. At any time, any combination of these failures is possible.

Calling `system.add_element_type()` registers common-cause failure, and calling `system.add_element()` registers independent failure.

The instances of `Voting` class takes references to elements and/or other outputs of other votings. Based on a given state (state defines which failures happened, and thus which elements are in the failed state),
voting can determine whether it has failed or not. This is evaluated recursively until it is determined whether the entire system failed or not.

On `system.compile()`, the system iterates through all possible states (combinations of elements failing or not failing), and evaluates voting to select and cache the states that lead to system failure. Upon calling `system.failure_probability()`, the system iterates through all states that lead to system failure and sums their probabilities at given time.

Currently the time complexity is $\mathcal{O}\left(\exp(n_c+n_i)\right)$ where $n_c$ is number of common-cause failures (element types) and $n_i$ is number of independent failures (elements). If needed, the complexity can be improved by
 - calculating probabilities for branches without common element types separately
 - optimizing votings that have only one kind of input
 - pruning states to eliminate very unlikely ones

### Usage

**The code for this example is also in Program.cs**

The usage will be demonstrated on the following scheme

```
[sensor1]---[valve1]---|
                       | (1oo2) ---|
[sensor2]--------------|           | (2oo2) ----> 
[sensor3]--------------------------|
```

First, initialize a system class:

```
SILSystem system = new SILSystem();
```

This class holds information about all elements for efficient calculation of probabilities.

Next, specify types of elements. In our case we have two distinct element types, `sensor` and `valve`. We also need to provide values for all parameters such as failure rates etc.

```
var sensor = system.add_element_type(new ElementType(
    // dangerous_detected, dangerous_undetected, proof_test_coverage, common_detected, common_undetected, mean_repair_time, mean_time_to_restore, proof_test_interval
    0.001, 0.0003, 0.8, 0.02, 0.01, 8, 8, 200
));

var valve = system.add_element_type(new ElementType(
    // dangerous_detected, dangerous_undetected, proof_test_coverage, common_detected, common_undetected, mean_repair_time, mean_time_to_restore, proof_test_interval
    0.001, 0.001, 0.6, 0.02, 0.01, 8, 8, 400
));
```

Once we have defined the element types, we can use them to create individual elements:

```
var sensor1 = system.add_element(sensor);
var sensor2 = system.add_element(sensor);
var sensor3 = system.add_element(sensor);
var valve1 = system.add_element(valve);
```

Now that we have all elements ready, we need to connect them.

If elements are connected in series, it is equivalent as connecting them in parallel without redundancy, this is what we need to do for `sensor1` and `valve1`;
 
```
// connecting elements in series is same as voting without redundancy
var connection1 = new Voting(
    // list of elements, list of votings, redundancy
    (new int[] {sensor1, valve1}).ToList(), (new Voting[] {}).ToList(), 0
);

var voting1oo2 = new Voting(
    // list of elements, list of votings, redundancy
    (new int[] {sensor2}).ToList(), (new Voting[] {connection1}).ToList(), 1
);

var voting2oo2 = new Voting(
    // list of elements, list of votings, redundancy
    (new int[] {sensor3}).ToList(), (new Voting[] {voting1oo2}).ToList(), 1
);
```

Finaly we need to specify which voting produces the final output and compile the system.

```
// specify which voting produces the final output
system.set_final_voting(voting2oo2);

// compile the system
system.compile();
```

Now the system is ready and it can calculate probabilities at certain time points, or average the probabilities over mission lifetime.

```
// now we can get failure probabilities at certain times
Console.WriteLine(
    "Failure probability after 1000 hrs: {0}",
    system.failure_probability(1000)
);

// and it is also possible to integrate and average the probability
Console.WriteLine(
    "Average failure probability after 1000 hrs: {0}",
    system.average_failure_probability(1000)
);
```


### Limitation on system size

Currently the program needs to consider all possible failure states, and there are exponentially many of them.

The practical limitation is around 20 - 30 element types and elements combined. If needed, the calculation can be optimized by ommiting very unlikely states.

