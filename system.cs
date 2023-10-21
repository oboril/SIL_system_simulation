namespace SystemSimulation
{
    using Probability = Double;
    using Rate = Double;
    using Time = Double;
    using ElementTypeID = Int32;
    using ElementID = Int32;
    using System.ComponentModel;

    /// <summary>
    /// Defines a single possible states with specific failure causes
    /// </summary>
    struct State
    {
        /// <summary>
        /// Bitmap, if (individual_failures >> idx & 1) the element is in failed state.
        /// </summary>
        public readonly ulong individual_failures;
        /// <summary>
        /// Bitmap, if (common_failures >> idx & 1) the element type is in failed state.
        /// </summary>
        public readonly ulong common_failures;

        /// <summary>
        /// Creates bitmap where (result >> idx) & 1 denotes whether given element is in failed state
        /// </summary>
        public static ulong all_element_states(ulong common_failures, ulong individual_failures, List<ElementTypeID> elements)
        {
            // start with individual failures
            ulong all_element_states = individual_failures;

            // add common failures
            for (int element = 0; element < elements.Count(); element++)
            {
                ulong failed = (common_failures >> elements[element]) & 1;
                all_element_states |= failed << element;
            }

            // return bitmap
            return all_element_states;
        }

        public State(ulong common_failures_, ulong individual_failures_)
        {
            common_failures = common_failures_;
            individual_failures = individual_failures_;
        }

        /// <summary>
        /// Calculates probability of this state
        /// </summary>
        public Probability probability(List<ElementType> element_types, List<ElementTypeID> elements, Time mission_time, Time time_since_proof_test)
        {
            Probability prob = 1.0;

            // Individual failures
            for (int element = 0; element < elements.Count(); element++)
            {
                Probability p = element_types[elements[element]].independent_failure(mission_time, time_since_proof_test);
                if (((individual_failures >> element) & 1) == 1)
                    prob *= p;
                else
                    prob *= 1.0 - p;    
            }


            // Common failuers
            for (int element_type = 0; element_type < element_types.Count(); element_type++)
            {
                Probability p = element_types[element_type].common_failure(mission_time, time_since_proof_test);
                if (((common_failures >> element_type) & 1) == 1)
                    prob *= p;
                else
                    prob *= 1.0 - p;    
            }


            return prob;
        }
    }

    class SILSystem
    {
        /// <summary>
        /// List of all element types. This stores failure probalities.
        /// </summary>
        List<ElementType> element_types;
        /// <summary>
        /// Types of all elements in the system.
        /// </summary>
        List<ElementTypeID> elements;
        /// <summary>
        /// Final voting that determines whether the system failed.
        /// </summary>
        Voting? final_voting;

        bool compiled;
        /// <summary>
        /// List of all significatly contributing states that cause failure
        /// </summary>
        List<State> fail_states;

        /// <summary>
        /// Creates empty SIL system class.
        /// </summary>
        public SILSystem()
        {
            element_types = new List<ElementType>();
            elements = new List<ElementTypeID>();
            final_voting = null;
            compiled = false;
            fail_states = new List<State>();
        }

        /// <summary>
        /// Adds new type of element to the SIL system
        /// </summary>
        /// <param name="element_type">The specifications of the element type</param>
        /// <returns>ID of the new element type</returns>
        public ElementTypeID add_element_type(ElementType element_type)
        {
            if (compiled)
                throw new Exception("Cannon add elements to compiled system");
            element_types.Add(element_type);
            return element_types.Count() - 1;
        }

        /// <summary>
        /// Adds element of given type to the SIL system.
        /// </summary>
        /// <param name="element_type_id">ID of the type of the element, as returned by add_element_type()</param>
        /// <returns>ID of the added element</returns>
        public ElementID add_element(ElementTypeID element_type_id)
        {
            if (compiled)
                throw new Exception("Cannon add elements to compiled system");
            elements.Add(element_type_id);
            return elements.Count() - 1;
        }

        /// <summary>
        /// Sets the final voting that determines whether the SIL system failed or not.
        /// </summary>
        /// <param name="voting">The final voting.</param>
        public void set_final_voting(Voting voting)
        {
            if (compiled)
                throw new Exception("Cannon change final voting of compiled system");
            final_voting = voting;
        }

        /// <summary>
        /// Compiles the SIL system, this is needs to be done before calculating probabilities.
        /// Compiling the SIL system significantly speeds up calculation of failure probabilities, which is key for fast integration.
        /// </summary>
        public void compile()
        {
            // Check that final voting is set
            if (! final_voting.HasValue)
                throw new Exception("The final voting needs to be specified before SIL system is compiled");

            // Check number of states
            int vars = element_types.Count() + elements.Count();
            if (vars > 40)
                throw new Exception("Using more than 40 elements and element types in SIL system will take forever.");
            if (vars > 20)
                Console.WriteLine("WARNING: the number of elements in SIL system is high {0}, this will cause compilation to be very slow", vars);
            
            // Cache all failing states
            for (ulong state_idx = 0; state_idx < (1ul << vars); state_idx++)
            {
                ulong common = state_idx & ((1ul << element_types.Count()) - 1);
                ulong individual = state_idx >> element_types.Count();

                ulong all_element_states = State.all_element_states(common, individual, elements);
                if (final_voting.Value.failed(all_element_states))
                {
                    State state = new State(common, individual);
                    fail_states.Add(state);
                }
            }

            // Set flags
            compiled = true;
        }

        public Probability failure_probability(Time mission_time, Time time_since_proof_test)
        {
            // Check that system is compiled 
            if (! compiled)
                throw new Exception("The system needs to be compiled before calculating probabilities");
            
            // Sum probabilities of all fail states
            Probability prob = 0.0;

            foreach (State state in fail_states)
                prob += state.probability(element_types, elements, mission_time, time_since_proof_test);

            return prob;
        }
    }
}