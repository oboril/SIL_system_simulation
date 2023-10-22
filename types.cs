namespace SystemSimulation
{
    using Probability = Double;
    using Rate = Double;
    using Time = Double;
    using ElementTypeID = Int32;
    using ElementID = Int32;

    struct ElementType
    {
        /// <summary>
        /// Rate of dangerous detected failures, both due to individual and common causes.
        /// Corresponding symbol is lambda_DD.
        /// </summary>
        Rate dangerous_detected;
        /// <summary>
        /// Rate of dangerous undetected failures, both due to individual and common causes.
        /// Note that some of these failures will be detected during proof tests.
        /// Corresponding symbol is lambda_DU
        /// </summary>
        Rate dangerous_undetected;
        /// <summary>
        /// Defines what fraction of dangerous undetected failures will be detected during proof tests.
        /// Corresponding symbol is PTC.
        /// </summary>
        Probability proof_test_coverage;
        /// <summary>
        /// Defines what fraction of undetected failures is due to common causes.
        /// Corresponding symbol is beta.
        /// </summary>
        Probability common_undetected;
        /// <summary>
        /// Defines what fraction of detected failures is due to common causes.
        /// Corresponding symbol is beta_D.
        /// </summary>
        Probability common_detected;
        /// <summary>
        /// Time required to repair detected failure.
        /// Corresponding symbol is MRT.
        /// </summary>
        Time mean_repair_time;
        /// <summary>
        /// Time required to repair undetected failure, presumably detected during proof tests.
        /// However, an approximation is made that assumes that the failures are repaired immidiately.
        /// Corresponding symbol is MTTR.
        /// </summary>
        Time mean_time_to_restore;

        /// <summary>
        /// Interval at which the components are tested for undetected faults
        /// </summary>
        public readonly Time proof_test_interval;

        public ElementType(Rate dangerous_detected_, Rate dangerous_undetected_, Probability proof_test_coverage_, Probability common_detected_, Probability common_undetected_, Time mean_repair_time_, Time mean_time_to_restore_, Time proof_test_interval_)
        {
            dangerous_detected = dangerous_detected_;
            dangerous_undetected = dangerous_undetected_;
            proof_test_coverage = proof_test_coverage_;
            common_detected = common_detected_;
            common_undetected = common_undetected_;
            mean_repair_time = mean_repair_time_;
            mean_time_to_restore = mean_time_to_restore_;
            proof_test_interval = proof_test_interval_;
        }

        Probability exponential_dist(Rate r, Time t)
        {
            Probability rt = r * t;
            if (rt < 1e-5)
                // Using Taylor expansion of 1 - exp(-rt) is most numerically stable
                return rt - rt * rt / 2 + rt * rt * rt / 6 - rt * rt * rt * rt / 24;
            else
                // for larger rt the exact formula is better
                return 1-Math.Exp(-rt);
        }

        /// <summary>
        /// Calculates probability that a single element of this type is in the failed state at given time.
        /// </summary>
        public Probability independent_failure(Time mission_time)
        {
            Time time_since_proof_test = mission_time % proof_test_interval;
            Probability combined = 0.0d;

            // Undetected failures
            combined += exponential_dist(dangerous_undetected * proof_test_coverage * (1 - common_undetected), time_since_proof_test);
            combined += exponential_dist(dangerous_undetected * (1 - proof_test_coverage) * (1 - common_undetected), mission_time);

            // Repair times
            combined += mean_repair_time * dangerous_detected * (1 - common_detected);
            combined += mean_time_to_restore * dangerous_undetected * (1 - common_undetected);

            return combined;
        }

        /// <summary>
        /// Calculates probability that all elements of this type is in the failed state at given time due to a common cause.
        /// </summary>
        public Probability common_failure(Time mission_time)
        {
            Time time_since_proof_test = mission_time % proof_test_interval;
            Probability combined = 0.0d;

            // Undetected failures
            combined += exponential_dist(dangerous_undetected * proof_test_coverage * common_undetected, time_since_proof_test);
            combined += exponential_dist(dangerous_undetected * (1 - proof_test_coverage) * common_undetected, mission_time);

            // Repair times
            combined += mean_repair_time * dangerous_detected * common_detected;
            combined += mean_time_to_restore * dangerous_undetected * common_undetected;

            return combined;
        }
    }

    struct Voting
    {
        /// <summary>
        /// Redundancy - how many inputs can fail without this voting failing.
        /// </summary>
        int redundancy;
        /// <summary>
        /// List of Element IDs that this voting takes as an input
        /// </summary>
        List<ElementID> elements;
        /// <summary>
        /// List of IDs of votings that this voting takes as an input
        /// </summary>
        List<Voting> votings;

        public Voting(List<ElementID> elements_, List<Voting> votings_, int redundancy_)
        {
            elements = elements_;
            votings = votings_;
            redundancy = redundancy_;
        }

        /// <summary>
        /// Determines whether the voting failed based on current inputs
        /// </summary>
        /// <param name="all_element_states">Bitmap, states whether each of the elements failed</param>
        /// <param name="all_votings">List of all votings in the system</param>
        /// <returns>Returns true if voting failed</returns>
        public bool failed(ulong all_element_states)
        {
            int failed_count = 0;

            foreach (ElementID id in elements)
                 if (((all_element_states >> id) & 1) == 1)
                    failed_count++;

            foreach (Voting voting in votings)
                 if (voting.failed(all_element_states))
                    failed_count++;

            if (failed_count > redundancy)
                return true;
            else
                return false;
        }
    }
}